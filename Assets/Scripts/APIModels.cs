using System;

[Serializable]
public class QuestionResponse
{
    public int questionId;
    public string type; // "TEXT", "IMAGE", "AUDIO", "VIDEO"
    public string content; // The text or the URL/Link to the media
    public string ruleId;
    public string ruleDescription; // e.g., "Reverse words..."
    public int currentQuestionIndex;
}

[Serializable]
public class AnswerRequest
{
    public int questionId;
    public string hashedAnswer;
}