using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MailKitSample.Services
{
    public class SampleJsomModel
    {
        [JsonPropertyName("@odata.context")]
        public string odatacontext { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string odataetag { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public object[] categories { get; set; }
        public DateTime receivedDateTime { get; set; }
        public DateTime sentDateTime { get; set; }
        public bool hasAttachments { get; set; }
        public string internetMessageId { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string parentFolderId { get; set; }
        public string conversationId { get; set; }
        public string conversationIndex { get; set; }
        public object isDeliveryReceiptRequested { get; set; }
        public bool isReadReceiptRequested { get; set; }
        public bool isRead { get; set; }
        public bool isDraft { get; set; }
        public string webLink { get; set; }
        public string inferenceClassification { get; set; }
        public MailBody MailBody { get; set; }
        public Sender sender { get; set; }
        public From from { get; set; }
        public Torecipient[] toRecipients { get; set; }
        public object[] ccRecipients { get; set; }
        public object[] bccRecipients { get; set; }
        public object[] replyTo { get; set; }
        public Flag flag { get; set; }
        public object[] attachments { get; set; }
    }

    public class MailBody
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class Sender
    {
        public Emailaddress emailAddress { get; set; }
    }

    public class Emailaddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class From
    {
        public Emailaddress1 emailAddress { get; set; }
    }

    public class Emailaddress1
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class Flag
    {
        public string flagStatus { get; set; }
    }

    public class Torecipient
    {
        public Emailaddress2 emailAddress { get; set; }
    }

    public class Emailaddress2
    {
        public string name { get; set; }
        public string address { get; set; }
    }
}