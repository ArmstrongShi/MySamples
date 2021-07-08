
namespace Advent.ApxSoap.Examples
{
    using Advent.ApxSoap.ApxWS;
    class ApxActivity
    {
        public static void GetAllActivities(ApxWS apxWS)

        {
            ActivityQueryOptions queryOptions = new ActivityQueryOptions();
            ActivityQueryResult queryResult;
            apxWS.Activity_GetAll(ref queryOptions, out queryResult);
        }

        /// <summary>
        /// This example shows how to create a new activity
        /// </summary>
        /// <param name="apxWS"></param>
        public static void CreateNewActivity(ApxWS apxWS)
        {
            Activity email = new Activity();
            email._DBAction = DBAction.Insert;
            email._UpdatedFields = new ActivityUpdatedFields();

            email.ActivityTypeID = "Email";
            email._UpdatedFields.ActivityTypeID = true;

            email.OwnerCode = "web";
            email.OwnerCodeIsNull = false;
            email._UpdatedFields.OwnerCode = true;

            email.EmailSenderAddress = "from@test.com";
            email.EmailSenderAddressIsNull = false;
            email._UpdatedFields.EmailSenderAddress = true;

            email.EmailRecipientListTo = "to@test.com;to2@test.com";
            email.EmailRecipientListToIsNull = false;
            email._UpdatedFields.EmailRecipientListTo = true;

            email.EmailRecipientListCC = "cc@test.com;cc2@test.com";
            email.EmailRecipientListCCIsNull = false;
            email._UpdatedFields.EmailRecipientListCC = true;

            email.EmailRecipientListBCC = "bcc@test.com;bcc2@test.com";
            email.EmailRecipientListBCCIsNull = false;
            email._UpdatedFields.EmailRecipientListBCC = true;

            email.EmailSubject = "subject";
            email.EmailSubjectIsNull = false;
            email._UpdatedFields.EmailSubject = true;

            email.EmailBodyText = "body text";
            email.EmailBodyTextIsNull = false;
            email._UpdatedFields.EmailBodyText = true;

            email.EmailSentTime = "2015-12-10 18:00:00Z";
            email.EmailSentTimeIsNull = false;
            email._UpdatedFields.EmailSentTime = true;

            email.EmailReceivedTime = "2015-12-10 18:01:00Z";
            email.EmailReceivedTimeIsNull = false;
            email._UpdatedFields.EmailReceivedTime = true;

            ActivityPutOptions ops = new ActivityPutOptions();
            ActivityPutResult pResult;
            Status status = apxWS.Activity_Put(ref ops, email, out pResult);
        }
    }
}
