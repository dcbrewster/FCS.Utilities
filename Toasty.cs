using System;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FCS.Utilities;
/////////////////////////////////////////////////////
///                                               ///
/// WARNING: This code is not production ready.   ///
/// It is provided as a sample for demonstration. ///
/// ////////////////////////////////////////////////

public class Toasty
{
    /// <summary>
    /// Create Toast Object
    /// </summary>
    public ToastContentBuilder toast = new();

    /// <summary>
    /// Build And Show Toast Message
    /// </summary>
    /// <param name="delay"></param>
    public void Show(int delay = 1)
    {
        Uri uri = new(Path.GetFullPath(@"Images\Fred1.jpg"));
        Uri logoUri = new(Path.GetFullPath(@"Images\Fred2.png"));
        Uri imageUri = new("https://picsum.photos/360/202?image=883");

        toast.AddArgument("action", "viewConversation")
            .AddArgument("conversationId", 9813)
            .AddText("Andrew sent you a picture")
            .AddText("Check this out, The Enchantments in Washington!")
            .AddInputTextBox("tbReply", placeHolderContent: "Type a response")
            .AddInlineImage(imageUri)                               // Inline image
            .AddInlineImage(uri)                                    // Inline image
            .AddButton(new ToastButton()
                .SetContent("Reply")
                .AddArgument("action", "reply")
                .SetBackgroundActivation())
            .AddButton(new ToastButton()
                .SetContent("Like")
                .AddArgument("action", "like")
                .SetBackgroundActivation())
        .AddButton(new ToastButton()
            .SetContent("View")
            .AddArgument("action", "viewImage")
            .AddArgument("imageUrl", imageUri.ToString()))
            .AddAppLogoOverride(logoUri, ToastGenericAppLogoCrop.Circle);

        // Schedule Toast Delivery - Default 1 Second
        toast.Schedule(DateTimeOffset.UtcNow + TimeSpan.FromSeconds(delay==0 ? 1 : delay));
    }

    /// <summary>
    /// Clear All Toast Messages
    /// </summary>
    public static void Clear() => ToastNotificationManagerCompat.History.Clear();

    /// <summary>
    /// Remove Messages Belonging To The Supplied Group
    /// </summary>
    /// <param name="groupName">The name of the group whose messages should be removed.</param>
    public static void Remove(string groupName)
    {
        toast.RemoveGroup(groupName);
    }

    /// <summary>
    /// Other Testing
    /// </summary>
    public static void Crap()
    {
        string imageUri = Path.GetFullPath(@"Images\Fred1.jpg");

        new ToastContentBuilder()
            .AddArgument("action", "viewConversation")
            .AddArgument("conversationId", 9813)
            .AddText("This Is THE title")
            .AddText("URL:" + imageUri)
            .AddAppLogoOverride(new Uri(imageUri))
            .AddHeroImage(new Uri(imageUri))
            .AddButton(new ToastButton()
                .SetContent("Open URL")
                .SetProtocolActivation(new Uri(imageUri))
            )
        .Show();
    }
}