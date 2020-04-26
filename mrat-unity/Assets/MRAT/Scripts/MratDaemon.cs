using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MRAT
{
	public class MratDaemon : MonoBehaviour
	{
		public string ServerBaseUri = "http://localhost:8081";
		public string ServerMessageUri = "/mrat/insert";
		public string ServerPhotoOnlyUri = "/mrat/saveScreenshot";
		public string ServerGetSessionIdsUri = "/mrat/getSessionIds";
		public string ServerGetEventsByIdUri = "/mrat/getEventsBySessionId";
		public string ServerInteractiveEndpoint = "/mrat/getEventSelection";
		public string ServerInteractiveUpdated = "/mrat/hasEventSelectionChanged";
		public string User = "user";
		public string Token = "abcd1234";
		public bool LogServerResponsesLocally = true;
		public bool LogWorkerProgress = true;

		/// <summary>
		/// System will wait until file is at least this old, in seconds, before trying to send, as a work-around to file lock errors.
		/// </summary>
		public int MinimumFileAgeSeconds = 3;
		
		private string _dataPath;
		private static HttpClient _client;
		private readonly ConcurrentQueue<MratEventSimple> _simpleEventBuffer = new ConcurrentQueue<MratEventSimple>();
		private readonly ConcurrentQueue<MratEventImage> _imageEventBuffer = new ConcurrentQueue<MratEventImage>();
		private bool _simpleEventWorkerRunning;
	    private bool _imageEventWorkerRunning;

		private void Start()
		{
			_dataPath = Application.persistentDataPath;
			_client = new HttpClient();

			InvokeRepeating(nameof(RunSimpleEventWorker), 1, 0.5f);

		    InvokeRepeating(nameof(RunImageEventWorker), 2, 2);
        }

		public async Task<List<MratEventSimple>> GetEventsBySessionId(string sessionId)
		{
			var stringResult = await GetEventsBySessionId(MratHelpers.GetServerPath(ServerBaseUri, ServerGetEventsByIdUri), sessionId, LogServerResponsesLocally);

			var listResult = await JsonEventsToSimpleEventList(stringResult);

			return listResult;
		}

		public async Task<List<MratEventSimple>> GetEventsFromInteractiveEndpoint()
		{
			var stringResult = await GetEvents(MratHelpers.GetServerPath(ServerBaseUri, ServerInteractiveEndpoint), LogServerResponsesLocally);

			var listResult = await JsonEventsToSimpleEventList(stringResult);

			return listResult;
		}

		public Task<bool> GetInteractiveDataStatus()
		{
			return GetInteractiveDataChangedStatus(MratHelpers.GetServerPath(ServerBaseUri, ServerInteractiveUpdated), LogServerResponsesLocally);
		}

		[SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
		private static async Task<List<MratEventSimple>> JsonEventsToSimpleEventList(string[] jsonEvents)
		{
			Debug.Log("Running JsonEventsToSimpleEventList");

			var task = Task.Run(() =>
			{
				var simpleEventList = new List<MratEventSimple>(jsonEvents.Length);

				MratEventSimple simpleEvent;
				JSONNode jsonNode;
				string baseType;

				// ReSharper disable once ForCanBeConvertedToForeach
				for (var i = 0; i < jsonEvents.Length; i++)
				{
					jsonNode = JSON.Parse(jsonEvents[i]);

					if (jsonNode == null)
					{
						Debug.Log("ERROR: jsonNode is null, indicating problem with this event: " + jsonEvents[i]);
						continue;
					}

					baseType = jsonNode["EventBaseType"]?.Value;

					if (baseType == null)
					{
						Debug.Log("ERROR: EventBaseType is null, problem with this event: " + jsonNode);
						continue;
					}
					else if (string.Equals(baseType, typeof(MratEventStatusUpdate).Name))
					{
						simpleEvent = JsonUtility.FromJson<MratEventStatusUpdate>(jsonEvents[i]);
					}
					else if (string.Equals(baseType, typeof(MratEventObjectUpdate).Name))
					{
						simpleEvent = JsonUtility.FromJson<MratEventObjectUpdate>(jsonEvents[i]);
					}
//					else if (string.Equals(baseType, typeof(MratEventTaskStatusUpdate).Name))
//					{
//						simpleEvent = JsonUtility.FromJson<MratEventTaskStatusUpdate>(jsonEvents[i]);
//					}
					else if (string.Equals(baseType, typeof(MratEventInput).Name))
					{
						simpleEvent = JsonUtility.FromJson<MratEventInput>(jsonEvents[i]);
					}
					else if (string.Equals(baseType, typeof(MratEventPhotoCaptured).Name))
					{
						simpleEvent = JsonUtility.FromJson<MratEventPhotoCaptured>(jsonEvents[i]);
					}
                    else
					{
						simpleEvent = JsonUtility.FromJson<MratEventSimple>(jsonEvents[i]);
                    }
                    

                    simpleEventList.Add(simpleEvent);
				}

				return Task.FromResult(simpleEventList);
			});

			var result = await task;

			return result;
		}

		public void SendSimpleEvent(MratEventSimple msg)
		{
			_simpleEventBuffer.Enqueue(msg);
		}

		public void SendImageEvent(MratEventImage msg)
		{
			_imageEventBuffer.Enqueue(msg);
		}

		private void RunSimpleEventWorker()
		{
			if (LogWorkerProgress) Debug.Log("RunSimpleEventWorker with _simpleEventWorkerRunning state: " + _simpleEventWorkerRunning);

			if (!_simpleEventWorkerRunning)
			{
				SimpleEventWorker();
			}
		}

	    private void RunImageEventWorker()
	    {
		    if (LogWorkerProgress) Debug.Log("RunImageEventWorker with _imageEventWorkerRunning state: " + _imageEventWorkerRunning);

			if (!_imageEventWorkerRunning)
	        {
	            ImageEventWorker();
	        }
	    }

	    [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
	    private void ImageEventWorker()
	    {
	        if (_imageEventWorkerRunning)
	        {
	            if (LogWorkerProgress) Debug.Log("ImageEventWorker already running, stopping here.");
	            return;
	        }

	        if (_imageEventBuffer.Count < 1)
	        {
	            if (LogWorkerProgress) Debug.Log("ImageEventWorker has nothing to do, ending.");
	            return;
	        }

	        _imageEventWorkerRunning = true;

	        if (LogWorkerProgress) Debug.Log("ImageEventWorker running, with _imageEventBuffer count: "
                                             + _imageEventBuffer.Count);

	        Task.Factory.StartNew(async () =>
	        {
		        var watch = Stopwatch.StartNew();

				try
				{
					MratEventImage eventImage;
		            MratImageMessage imageMsg;

                    while (_imageEventBuffer.TryDequeue(out eventImage))
			        {
				        if (eventImage == null)
				        {
					        Debug.Log("_imageEventBuffer.TryDequeue gave null message, skipping to next");
					        continue;
				        }

						imageMsg = new MratImageMessage(eventImage);

				        if (!File.Exists(Path.Combine(_dataPath, eventImage.ImageFileName)))
				        {
					        _imageEventBuffer.Enqueue(eventImage);
					        if (LogWorkerProgress) Debug.Log("ImageEventWorker: file not available yet, " +
					                                         "so returning image to queue with name: " + imageMsg.FileName);
					        continue;
				        }

				        var fileCreationTime = File.GetCreationTime(Path.Combine(_dataPath, eventImage.ImageFileName));
				        var ageSeconds = (DateTime.Now - fileCreationTime).TotalSeconds;

						if (ageSeconds < MinimumFileAgeSeconds)
				        {
							// File is too 'fresh', so there is a greater chance it is still in use from image being captured.
							// To avoid an IOError from file being in use, let's skip this file and come back later.
					        _imageEventBuffer.Enqueue(eventImage);
					        if (LogWorkerProgress) Debug.Log($"ImageEventWorker: file is too new, with age {ageSeconds}, " +
					                                         $"so returning image to queue with name: {imageMsg.FileName}");

					        if (_imageEventBuffer.Count == 1)
					        {
								// The item re-added to the queue must be this one, so exit the loop and get it on the next run.
						        break;
					        }

					        continue;
						}

						if (LogWorkerProgress) Debug.Log("ImageEventWorker: Preparing to send image with filename: " + imageMsg.FileName);

				        var serverResult = await PostImageMessageAsync(imageMsg, MratHelpers.GetServerPath(ServerBaseUri, ServerPhotoOnlyUri), LogServerResponsesLocally);

						// TODO: If file sent successfully, delete it from local file system.

				        if (serverResult) continue;

				        _imageEventBuffer.Enqueue(eventImage);

				        Debug.Log("ImageEventWorker: a PostServerMessageAsync result failed, image not sent: " + imageMsg.FileName);
				        
						break;
			        }

				}
				catch (Exception e)
		        {
			        Debug.Log("ImageEventWorker: Error in task: " + e);
		        }

	            _imageEventWorkerRunning = false;

				watch.Stop();

		        if (LogWorkerProgress) Debug.Log("ImageEventWorker: Task completed, running time: " + watch.ElapsedMilliseconds);
			}, TaskCreationOptions.LongRunning);
	    }

        [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
		private void SimpleEventWorker()
		{
			if (_simpleEventWorkerRunning)
			{
				if (LogWorkerProgress) Debug.Log("SimpleEventWorker already running, stopping here.");
				return;
			}

			if (_simpleEventBuffer.Count < 1)
			{
				if (LogWorkerProgress) Debug.Log("SimpleEventWorker has nothing to do, ending.");
				return;
			}

			_simpleEventWorkerRunning = true;

			if (LogWorkerProgress) Debug.Log("SimpleEventWorker running, with _simpleEventBuffer count: " 
			                                 + _simpleEventBuffer.Count);

			Task.Factory.StartNew(async () =>
			{
				var watch = Stopwatch.StartNew();

				try
				{
					MratEventSimple simpleEvent;
				    var serverMsg = new MratServerMessage(User, Token);

                    var eventList = new List<MratEventSimple>(_simpleEventBuffer.Count);

					while (_simpleEventBuffer.TryDequeue(out simpleEvent))
					{
						if (simpleEvent == null)
						{
							Debug.Log("_simpleEventBuffer.TryDequeue gave null message, skipping to next");
							continue;
						}

						eventList.Add(simpleEvent);
                        serverMsg.JsonMessages.Add(simpleEvent.ToString());

						// limit messages to be sent at one time to 10, to avoid uri too long error
						if (eventList.Count > 60)
						{
							if (LogWorkerProgress) Debug.LogWarning("Max message size exceeded, current event buffer size: " + _simpleEventBuffer.Count);
							break;
						}
					}

				    if (eventList.Count < 1)
				    {
                        Debug.Log("SimpleEventWorker Task: No events to send, exiting task.");
				        return;
				    }

				    if (LogWorkerProgress) Debug.Log("SimpleEventWorker Task: Preparing to send message: " 
				                                     + string.Join(";", serverMsg.JsonMessages));

				    var serverResult = await PostServerMessageAsync(
					    serverMsg, 
					    MratHelpers.GetServerPath(ServerBaseUri, ServerMessageUri), 
					    LogServerResponsesLocally);

				    if (!serverResult)
				    {
				        Debug.Log("SimpleEventWorker: a PostServerMessageAsync result failed, message not sent: "
				                  + string.Join(";", serverMsg.JsonMessages));

						Debug.LogError("Failed message list size: " + eventList.Count);

                        // Re-add all the events to the queue to try to send again later.
                        foreach (var eventSimple in eventList)
				        {
				            _simpleEventBuffer.Enqueue(eventSimple);
                        }
                    }
                }
                catch (Exception e)
				{
					Debug.Log("SimpleEventWorker: Error in task: " + e);
				}
				
				_simpleEventWorkerRunning = false;

				watch.Stop();

				if (LogWorkerProgress) Debug.Log("SimpleEventWorker: Task completed, running time: " + watch.ElapsedMilliseconds);
			}, TaskCreationOptions.LongRunning);
		}

		private static async Task<bool> GetInteractiveDataChangedStatus(string path, bool logServerResponseLocally = true)
		{
			try
			{
				var response = await _client.GetAsync(path);

				if (response.IsSuccessStatusCode)
				{
					if (logServerResponseLocally) Debug.Log("GetIfInteractiveDataChanged changed: " + response.StatusCode);

					return true;
				}

				if (logServerResponseLocally) Debug.Log($"GetIfInteractiveDataChanged not changed - Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");

				return false;
			}
			catch (Exception e)
			{
				Debug.Log("GetSessionIds error: " + e);
			}

			return false;
		}

		private static async Task<string[]> GetEvents(string path, bool logServerResponseLocally)
		{
			string[] finalResult = null;

			try
			{
				var response = await _client.GetAsync(path);

				if (response.IsSuccessStatusCode)
				{
					var rawResult = await response.Content.ReadAsStringAsync();

					if (logServerResponseLocally) Debug.Log($"GetEvents response with length {rawResult.Length}: {rawResult}");

					finalResult = RawJsonArrayServerResponseToArrayOfJsonStrings(rawResult);
				}

				if (logServerResponseLocally) Debug.Log($"GetEvents - Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
			}
			catch (Exception e)
			{
				Debug.Log("GetEvents error: " + e);
			}

			return finalResult;
		}

		private static async Task<string[]> GetEventsBySessionId(string path, string sessionId, bool logServerResponseLocally)
		{
			string[] finalResult = null;

			path += "?sessionId=" + WebUtility.UrlEncode(sessionId);

			Debug.Log("GetEventsBySessionId called with path: " + path);
			
			try
			{
				var response = await _client.GetAsync(path);

				if (response.IsSuccessStatusCode)
				{
					var rawResult = await response.Content.ReadAsStringAsync();

					if (logServerResponseLocally) Debug.Log("Daemon GetEventsBySessionId response: " + rawResult);

					finalResult = RawJsonArrayServerResponseToArrayOfJsonStrings(rawResult);
				}

				if (logServerResponseLocally) Debug.Log($"GetEventsBySessionId - Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
			}
			catch (Exception e)
			{
				Debug.Log("GetEventsBySessionId error: " + e);
			}

			return finalResult;
		}

		/// <summary>
		/// Make-shift "parser" of array of JSON objects received from server. Ugly, but it works to return
		/// an array of proper Json strings for more parsing.
		/// </summary>
		/// <param name="rawEvents"></param>
		/// <returns></returns>
		private static string[] RawJsonArrayServerResponseToArrayOfJsonStrings(string rawEvents)
		{
            /* NOTE: This is a highly implementation-specific hack, to work around the limited availability
			 of proper JSON parsing library that works in Unity and in UWP/HoloLens, and is high-performance.            
            */

			if (rawEvents.Length <= 0) return null;

			var trimmedString = rawEvents.Substring(1, rawEvents.Length - 2);

			var stringArray = trimmedString.Split(new[] { ",{\"_id\"" }, StringSplitOptions.RemoveEmptyEntries);

			if (stringArray.Length <= 1)
			{
				return new[] { trimmedString};
			}

			var stringList = new List<string>(stringArray.Length)
			{
				stringArray[0]
			};

			stringList.AddRange(stringArray.Skip(1).Select(str => "{\"_id\"" + str));

			return stringList.ToArray();
		}

		private async Task<bool> PostImageMessageAsync(MratImageMessage msg, string path, bool logServerResponseLocally = true)
		{
			if (LogWorkerProgress) Debug.Log("PostImageMessageAsync called with endpoint: " + path);

			try
			{
				using (var stream = File.OpenRead(Path.Combine(_dataPath, msg.FileName)))
				{
					HttpContent filenameContent = new StringContent(msg.FileName);
					HttpContent streamContent = new StreamContent(stream);

					var form = new MultipartFormDataContent
					{
						{filenameContent, "fileName"},
						{streamContent, "screenshot", msg.FileName}
					};

					var response = await _client.PostAsync(path, form);

					if (response.IsSuccessStatusCode)
					{
						var result = await response.Content.ReadAsStringAsync();

						if (logServerResponseLocally) Debug.Log("PostImageMessageAsync response: " + result);

						return true;
					}

					Debug.Log($"PostImageMessageAsync failed - Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
				}
			}
			catch (IOException e)
			{
				Debug.Log("PostImageMessageAsync IOException error, file probably still in use: " + e);
			}
			catch (Exception e)
			{
				Debug.Log("PostImageMessageAsync PostAsync error: " + e);
			}

			return false;
		}

		private async Task<bool> PostServerMessageAsync(MratServerMessage msg, string path, bool logServerResponseLocally = true)
		{
			if (LogWorkerProgress) Debug.Log("PostServerMessageAsync called");

			try
			{
				HttpContent encodedContent = new FormUrlEncodedContent(msg.ToDictionary());

				var response = await _client.PostAsync(path, encodedContent);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadAsStringAsync();

					if (logServerResponseLocally) Debug.Log("PostServerMessageAsync response: " + result);

					return true;
				}

				Debug.Log($"PostServerMessageAsync failed - Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
			}
			catch (Exception e)
			{
				Debug.Log("PostServerMessageAsync PostAsync error: " + e);
			}

			return false;
		}

		#region ReservedForFutureUse

		private static async Task<string> GetSessionIds(string path, bool logServerResponseLocally = true)
		{
			Debug.Log("GetSessionIds called");

			try
			{
				var response = await _client.GetAsync(path);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadAsStringAsync();

					if (logServerResponseLocally) Debug.Log("GetSessionIds response: " + result);

					return result;
				}

				Debug.Log($"GetSessionIds - Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
			}
			catch (Exception e)
			{
				Debug.Log("GetSessionIds error: " + e);
			}

			return null;
		}

		#endregion

	}
}