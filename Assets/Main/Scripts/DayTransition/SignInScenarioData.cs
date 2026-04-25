using System;

namespace NarrativeGP.DayTransition
{
    [Serializable]
    public class SignInScenarioData
    {
        public string scenarioId;
        public string field1Label = "User ID";
        public string field1TargetText = "0123";
        public string field2Label = "Password";
        public string field2TargetText = "****";
        public string checkboxLabel = "I'm not robot";
        public int requiredInputCount = 8;
        public SignInCompletionMode completionMode = SignInCompletionMode.AdvanceDayToAttendance;
    }
}
