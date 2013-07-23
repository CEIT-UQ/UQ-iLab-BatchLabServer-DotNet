
namespace Library.Lab
{
    public class TestConsts
    {
        /*
         * String constants
         */
        public static string STR_Ok = "OK.";
        public static string STR_TestSuccessful = "Test Successful.";
        /*
         * String constants for exception messages
         */
        public static string STRERR_ExceptionNotThrown = "Exception was not thrown: ";
        public static string STRERR_ExceptionWasThrown = "Exception should not have been thrown!";
        public static string STRERR_NullReferenceExceptionNotThrown = STRERR_ExceptionNotThrown + "NullReferenceException";
        public static string STRERR_ArgumentExceptionNotThrown = STRERR_ExceptionNotThrown + "ArgumentException";
        public static string STRERR_ArgumentNullExceptionNotThrown = STRERR_ExceptionNotThrown + "ArgumentNullException";
        public static string STRERR_FormatExceptionNotThrown = STRERR_ExceptionNotThrown + "FormatException";
        public static string STRERR_XmlUtilitiesExceptionNotThrown = STRERR_ExceptionNotThrown + "XmlUtilitiesException";

    }
}
