using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

namespace Meditation.UI.Controllers.Utils
{
    internal static class DialogUtilities
    {
        public static Task<ButtonResult> ShowMessageBox(string title, string content, ButtonEnum buttonEnum = ButtonEnum.Ok)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(title: title, text: content, @enum: buttonEnum);
            return messageBox.ShowAsync();
        }

        public static Task ShowUnhandledExceptionMessageBox(Exception exception)
        {
            return ShowMessageBox(title: "Unhandled exception", content: exception.ToString());
        }

        public static Task ShowUnhandledErrorMessageBox(string error)
        {
            return ShowMessageBox(title: "Unhandled exception", content: error);
        }
    }
}
