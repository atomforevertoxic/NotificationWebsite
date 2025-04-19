namespace NotificationWebsite.Data
{
    public static class ServiceStateLogText
    {
        public const string UserCreationSuccess = "The user was successfully subscribed";
        
        public const string DuplicateEmailError = "The user with this email is already subscribed";
        
        public const string DatabaseAccessError = "Database access error. User is not subscribed";
        
        public const string UserSavingError = "Error saving user to database. User is not subscribed";
        
        public const string ScheduleConfigurationError = "Error creating notification schedule. User is subscribed";
        
        public const string EmailSendingError = "Error sending email. User is subscribed";

        public const string OtherError = "An unknown error occurred while subscribing the user";
    }
}
