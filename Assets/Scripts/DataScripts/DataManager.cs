using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Static utility class used to communicate with the backend of the System
/// Requests are handled asynchronously so as to not stall the game while requests are being processed
/// Requests update the local data cache in CurrentUser when complete, and calls a notify method in the calling class to make use of the data
/// All requests require a token that is returned from the backend upon successful login
/// </summary>
public static class DataManager
{
    private static bool? requestStatus = null; // null indicates no request, bool for status of request
    private static string serverURL = "https://cz3003.kado.sg/api";
    public static string token;
    private static string userId;
    /// <summary>
    /// We declare a single string to contain the json payload, since only a single request can be sent at a time
    /// </summary>
    private static string jsonData = "";
    private static JObject tempObj =  new JObject();
    /// <summary>
    /// We declare a single UnityWebRequest variable to be reused, since only a single request can be sent at a time
    /// </summary>
    private static UnityWebRequest request;
    private static string response;
    public delegate void NotifyDelegate();
    
    /////////////
    // Request //
    /////////////

    /// <summary>
    /// Private method for creating a UnityWebRequest that contains a JSON payload
    /// </summary>
    /// <param name="URL">url to send the request to</param>
    /// <param name="type">type of request, can be "POST" or "PUT"</param>
    private static void makeJsonRequest(string URL, string type)
    {
        request = new UnityWebRequest (URL, type);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // reset data variables once request has been created
        tempObj = new JObject(); 
        jsonData = "";
    }

    /// <summary>
    /// Private method for creating a UnityWebrequest that does NOT contain a JSON payload
    /// </summary>
    /// <param name="URL">url to send the request to</param>
    /// <param name="type">type of request, can be "POST" or "PUT"</param>
    private static void makeRequest(string URL, string type)
    {
        request = new UnityWebRequest (URL, type);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // reset data variables once request has been created
        tempObj = new JObject();
        jsonData = "";
    }
    
    /// <summary>
    /// Checks the status of the latest request.
    /// Request status resets every time this method is called.
    /// </summary>
    /// <returns>null if request has not been processed, status of the request as a bool if request has been completed, and response received</returns>
    public static bool? getRequestStatus()
    {
        bool? temp = requestStatus;
        requestStatus = null; // once request has been acknowledged, reset status
        return temp;
    }

    /// <summary>
    /// Used to set request status after a request has been completed
    /// </summary>
    /// <param name="newStatus">Status of the request</param>
    public static void setRequestStatus(bool? newStatus)
    {
        requestStatus = newStatus;
    }

    /////////////
    // Account //
    /////////////

    /// <summary>
    /// Send and process a login request
    /// </summary>
    /// <param name="username">Case sensitive</param>
    /// <param name="password">Case sensitive</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator login(string username, string password, NotifyDelegate notifyMethod)
    {
        tempObj["username"] = username;
        tempObj["password"] = password;
        jsonData = tempObj.ToString();
        makeJsonRequest(serverURL + "/account/login/", "POST");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {            
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            User loggedIn = JsonConvert.DeserializeObject<User>(response);
            if (loggedIn.token != null) token = "Token "+loggedIn.token;
            userId = loggedIn.id.ToString();
            CurrentUser.loggedIn = loggedIn;
            Debug.Log(token);
            setRequestStatus(true);
        }
        notifyMethod();
    
    }
    /// <summary>
    /// Sends a request to change password of the current user on a first time login
    /// </summary>
    /// <param name="username">Case sensitive</param>
    /// <param name="password">Case sensitive - Old password</param>
    /// <param name="newPassword">Case sensitive - Password to change to</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator changePassword(string username, string password, string newPassword, NotifyDelegate notifyMethod)
    {
        tempObj["username"] = username;
        tempObj["password"] = password;
        tempObj["new_password"] = newPassword;
        jsonData = tempObj.ToString();
        makeJsonRequest(serverURL + "/account/changepassword/", "POST");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            CurrentUser.errorMessage = JsonConvert.DeserializeObject<badPasswordResponse>(response).detail[0];
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            bool? status = JsonConvert.DeserializeObject<ChangePasswordResponse>(response).success;
            setRequestStatus(status);
        }
        notifyMethod();
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class ChangePasswordResponse
    {
        public bool success;
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class badPasswordResponse
    {
        public string[] detail;
    }

    /// <summary>
    /// Send a logout request  to the server for the currently logged in user
    /// Sets loggedIn in CurrentUser
    /// </summary>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator logout(NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/account/logout/", "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            // Clear token and current user on logout
            CurrentUser.loggedIn = null;
            token = null;
            userId = null;
            setRequestStatus(true);
        }
        notifyMethod();
    }

    ////////////////////////////////
    // Standard Question & Answer //
    ////////////////////////////////

    /// <summary>
    /// Retrieve new questions for the current level of a specific World
    /// Sets currentQuestions, levelScore and correctCount in GameData
    /// </summary>
    /// <param name="world_id">World to retrieve questions for</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getQuestionData(int world_id, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/questions/world?world_id=" + world_id.ToString(), "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            // Deserialize JSON data and save to game data cache in CurrentUser
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            QuestionResponse respObj = JsonConvert.DeserializeObject<QuestionResponse>(response);
            GameData.currentQuestions = respObj.questions;
            GameData.levelScore = respObj.score;
            GameData.correctCount = respObj.correct_counter;
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class QuestionResponse
    {
        public Question[] questions;
        public int score;
        public int correct_counter;
    }

    /// <summary>
    /// Send a User's chosen answer to the backend for evaluation
    /// Sets currentQuestion in GameData with the result
    /// </summary>
    /// <param name="recordId">id of the question that the User has answered</param>
    /// <param name="answerId">id of the answer that the User has chosen</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator answerQuestion(int recordId, int answerId, NotifyDelegate notifyMethod)
    {
        tempObj["record_id"] = recordId;
        tempObj["answer_id"] = answerId;
        JArray tempList = new JArray();
        tempList.Add(tempObj);
        tempObj = new JObject(new JProperty("question_records", tempList));
        jsonData = tempObj.ToString();

        Debug.Log(jsonData);
        makeJsonRequest(serverURL + "/questions/world/check/", "POST");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log(response);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log(response);
            questionResult[] answerStatus = JsonConvert.DeserializeObject<questionResultResponse>(response).results;
            GameData.currentQuestion.answered_correctly = answerStatus[0].is_correct;
            GameData.currentQuestion.points_change = answerStatus[0].points;
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class answerToSend
    {
        public int record_id;
        public int answer_id;

        public answerToSend(int record_id, int answer_id)
        {
            this.record_id = record_id;
            this.answer_id = answer_id;
        }
    }

    /// <summary>
    /// Send a list of the User's chosen answers to the backend for evaluation
    /// Sets bossCorrectQuestions in GameData with the result
    /// </summary>
    /// <param name="recordIds"></param>
    /// <param name="answerIds"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator answerBossQuestions(int[] recordIds, int[] answerIds, NotifyDelegate notifyMethod)
    {
        JArray tempList = new JArray();
        for(int i = 0; i<recordIds.Length; i++)
        {
            tempObj = new JObject();
            tempObj["record_id"] = recordIds[i];
            tempObj["answer_id"] = answerIds[i];
            tempList.Add(tempObj);
        }
        tempObj = new JObject(new JProperty("question_records", tempList));
        jsonData = tempObj.ToString();
        Debug.Log(jsonData);

        makeJsonRequest(serverURL + "/questions/world/check/", "POST");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log(response);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            questionResult[] answerStatusArr = JsonConvert.DeserializeObject<questionResultResponse>(response).results;
            int correctCount = 0;
            foreach (questionResult result in answerStatusArr)
            {
                if (result.is_correct == true) correctCount++;
            }
            GameData.bossCorrectQuestions = correctCount;
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class question_record
    {
        public int record_id;
        public int answer_id;
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class questionResultResponse
    {
        public questionResult[] results;
    }

    public class questionResult
    {
        public int record_id;
        public string question_text;
        public bool is_correct;
        public int points;
    }

    /////////////////////////
    // Question and Answer //
    /////////////////////////

    /// <summary>
    /// Gets all custom questions created by the current logged in User
    /// </summary>
    /// <param name="worldId"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getCustomQuestions(int worldId, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/questions/?world_id=" + worldId.ToString(), "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            EditData.customQuestions = JsonConvert.DeserializeObject<Question[]>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// sends a created question to the server, requires a world to be created first
    /// section id field within toSend must correspond to the created custom world sectionid
    /// </summary>
    /// <param name="toSend"></param>
    /// <param name="sectionId"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator sendQuestion(Question toSend, int sectionId, NotifyDelegate notifyMethod)
    {      
        JArray tempList = new JArray();
        for(int i =0; i<4; i++)
        {
            tempObj = new JObject();
            tempObj["answer"] = toSend.answers[i].answer;
            tempObj["is_correct"] = toSend.answers[i].is_correct;
            tempList.Add(tempObj);
        }
        tempObj = new JObject(new JProperty("answers", tempList));
        tempObj["question"] = toSend.question;
        tempObj["section"] = sectionId;
        jsonData = tempObj.ToString();
        makeJsonRequest(serverURL + "/questions/", "POST");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// For creating custom questions in a Challenge to send to backend
    /// API requires only questionText, array of answers and section, the rest is generated by backend
    /// </summary>
    public class CreatedQuestion
    {
        public string question;
        public CreatedAnswer[] answers;
        public int section;

        public CreatedQuestion(string questionText)
        {
            this.question = questionText;
            this.section = 1;
        }
    }

    /// <summary>
    /// For creating custom answers in a Challenge to send to backend
    ///API requires only answerText and whether the answer is correct, the rest is generated by backend
    /// </summary>
    public class CreatedAnswer
    {
        public string answerText;
        public bool is_correct;

        public CreatedAnswer(string answerText, bool is_correct)
        {
            this.answerText = answerText;
            this.is_correct = is_correct;
        }
    }

    /// <summary>
    /// Retrieves a single custom question by id for editing
    /// sets editQuestion in EditData
    /// </summary>
    /// <param name="questionId"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getCustomQuestion(int questionId, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/questions/" + questionId.ToString() + "/", "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            EditData.editQuestion = JsonConvert.DeserializeObject<Question>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }
 
    /// <summary>
    /// Updates question text of a custom question by id
    /// </summary>
    /// <param name="toSend"></param>
    /// <param name="questionId"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator updateQuestion(Question toSend, int questionId, NotifyDelegate notifyMethod)
    {
        JArray tempList = new JArray();
        for(int i =0; i<4; i++)
        {
            tempObj = new JObject();
            tempObj["answer"] = toSend.answers[i].answer;
            tempObj["is_correct"] = toSend.answers[i].is_correct;
            tempList.Add(tempObj);
        }
        tempObj = new JObject(new JProperty("answers", tempList));
        tempObj["question"] = toSend.question;
        jsonData = tempObj.ToString();
        makeJsonRequest(serverURL + "/questions/" + toSend.id.ToString() + "/", "PUT");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Deletes a custom question by id
    /// </summary>
    /// <param name="questionId"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator deleteQuestion(int questionId, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/questions/" + questionId.ToString() + "/", "DELETE");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            setRequestStatus(true);
        }
        notifyMethod();
    }


    ///////////////////
    // User Position //
    ///////////////////

    /// <summary>
    /// Retrieves user's current position in a World
    /// sets currentWorldId, currentSectionId, currentLevelId, hasCompleted in CurrentUser
    /// </summary>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    /// <param name="worldId">Optional parameter, defaults to fetching current position if not provided</param>
    public static IEnumerator getCurrentPosition(NotifyDelegate notifyMethod, int worldId = -1)
    {
        Debug.Log("Fetching worldId: " + worldId);
        if (worldId != -1)
        {
            makeRequest(serverURL + "/position/?world_id="+worldId.ToString(), "GET");
        }
        else
        {
            makeRequest(serverURL + "/position/", "GET");
        }
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Position positionObj = JsonConvert.DeserializeObject<Position>(response);
            Debug.Log("getting position: " + response);
            CurrentUser.currentWorldId = positionObj.world_id;
            CurrentUser.currentSectionId = positionObj.section_id;
            CurrentUser.currentLevelId = positionObj.level_id;
            CurrentUser.hasCompleted = positionObj.has_completed;
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    public class Position
    {
        public int world_id;
        public int section_id;
        public int level_id;
        public bool has_completed;
    }

    //////////////////
    // Custom World //
    //////////////////

    /// <summary>
    /// Retrieves the list of Challenge Mode Worlds that the User has created
    /// </summary>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getCustomWorldList(NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/worlds/custom/", "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            EditData.customWorldList = JsonConvert.DeserializeObject<World[]>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Create a new Challenge Mode World in the backend to be saved to the database
    /// sets cutomWorld in EditData with the created World
    /// </summary>
    /// <param name="world_name">Name of the World</param>
    /// <param name="topic">Topic that the World's questions will be on</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator createWorld(string world_name, string topic, NotifyDelegate notifyMethod)
    {
        tempObj["world_name"] = world_name;
        tempObj["topic"] = topic;
        jsonData = tempObj.ToString();
        makeJsonRequest(serverURL + "/worlds/custom/", "POST");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            EditData.customWorld = JsonConvert.DeserializeObject<World>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Retrieves a single Challenge Mode World by access code
    /// sets customWorld in EditData with the retrieved World
    /// </summary>
    /// <param name="accessCode">Access code of the World to be retrieved</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    /// <param name="type">Can be "assignment" or "challenge", which will only retrieve worlds of specified type</param>
    public static IEnumerator getCustomWorld(string accessCode, NotifyDelegate notifyMethod, string type)
    {
        makeRequest(serverURL + "/worlds/custom/" + accessCode + "/?type=" + type, "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            EditData.customWorld = JsonConvert.DeserializeObject<World>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Updates a Challenge Mode World that the user has created
    /// </summary>
    /// <param name="accessCode">Access code of the World to be updated</param>
    /// <param name="worldName">Name to change to</param>
    /// <param name="topic">Topic to change to</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator updateCustomWorld(string accessCode, string worldName, string topic, NotifyDelegate notifyMethod)
    {
        tempObj["world_name"] = worldName;
        tempObj["topic"] = topic;
        jsonData = tempObj.ToString();
        makeJsonRequest(serverURL + "/worlds/custom/" + accessCode + "/", "PUT");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Deletes a Challenge Mode World by access code
    /// </summary>
    /// <param name="accessCode">Access code of the World to be deleted</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator deleteCustomWorld(string accessCode, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/worlds/custom/" + accessCode + "/", "DELETE");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /////////////////////
    // Standard Worlds //
    /////////////////////

    /// <summary>
    /// Retrieves all Campaign Worlds
    /// sets campaignWorlds in GameData
    /// </summary>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getCampaignWorlds(NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/worlds/", "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log(response);
            GameData.campaignWorlds = JsonConvert.DeserializeObject<World[]>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Retrieves a single World by worldId
    /// sets currentWorld in GameData
    /// </summary>
    /// <param name="worldId">world id of the World to be retrieved</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getWorld(int worldId, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/worlds/"+ worldId.ToString() + "/", "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            GameData.currentWorld = JsonConvert.DeserializeObject<World>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /////////////////
    // Leaderboard //
    /////////////////

    /// <summary>
    /// Retrieves the leaderboard containing total scores of all Students for all Worlds
    /// sets leaderboard in LeaderboardData
    /// </summary>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getOverallLeaderboard(NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/leaderboard/", "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            LeaderboardData.leaderboard = JsonConvert.DeserializeObject<Result[]>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Retrieves the leaderboard containing total scores of all Students for a single World
    /// sets leaderboard in LeaderboardData
    /// </summary>
    /// <param name="world_id">world id of the World to retrieve the leaderboard for</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getWorldLeaderboard(string world_id, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/leaderboard/?world_id=" + world_id, "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            LeaderboardData.leaderboard = JsonConvert.DeserializeObject<Result[]>(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }

    /// <summary>
    /// Retrieves total score of a single User, for all Worlds
    /// </summary>
    /// <param name="user_id"></param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getUserResult(int user_id, NotifyDelegate notifyMethod)
    {
        Debug.Log(user_id);
        makeRequest(serverURL + "/leaderboard/?user_id="+user_id.ToString(), "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        Debug.Log("Total Score Response");
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log(response);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            LeaderboardData.userResult = JsonConvert.DeserializeObject<Result>(response);
            Debug.Log(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }
    
    ////////////
    //  Misc  //
    ////////////

    /// <summary>
    /// Retrieves Score of the current User for a specific World
    /// </summary>
    /// <param name="worldId">world id to retrieve score for</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getWorldScore(int worldId, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/score/?world_id=" + worldId.ToString(), "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        Debug.Log("World Score Response");
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            Debug.Log(response);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            CurrentUser.currentScore = JsonConvert.DeserializeObject<Score>(response).points;
            Debug.Log(response);
            setRequestStatus(true);
        }
        notifyMethod();
    }
    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    private class Score
    {
        public int points;
    }

    /// <summary>
    /// Retrieves difficulty of the questions for the current User in the specified World
    /// </summary>
    /// <param name="worldId">world id of the World to retrieve difficulty for</param>
    /// <param name="notifyMethod">Method to be called upon completion of the request</param>
    public static IEnumerator getWorldDifficulty(int worldId, NotifyDelegate notifyMethod)
    {
        makeRequest(serverURL + "/difficulty/?world_id="+ worldId.ToString(), "GET");
        request.SetRequestHeader("Authorization", token);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            setRequestStatus(false);
        }
        else
        {
            response = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            CurrentUser.currentDifficulty = JsonConvert.DeserializeObject<Difficulty>(response).difficulty_text;
            setRequestStatus(true);
        }
        notifyMethod();
    }
    /// <summary>
    /// Class for formatting of received JSON payload
    /// </summary>
    private class Difficulty
    {
        public int difficulty;
        public string difficulty_text;
    }
}
