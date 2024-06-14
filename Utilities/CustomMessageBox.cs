using System.Windows;

namespace Panoramas_Editor
{
    static public class CustomMessageBox
    {
        public static MessageBoxResult ShowQuestion(string text, string caption = "Вопрос")
        {
            return MessageBox.Show(text,
                                   caption,
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Question,
                                   MessageBoxResult.None);
        }

        public static void ShowError(string text, string caption = "Ошибка")
        {
            MessageBox.Show(text,
                            caption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.None);
        }

        public static void ShowWarning(string text, string caption = "Внимание")
        {
            MessageBox.Show(text,
                            caption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning,
                            MessageBoxResult.None);
        }

        public static void ShowMessage(string text, string caption = "Сообщение")
        {
            MessageBox.Show(text,
                            caption,
                            MessageBoxButton.OK,
                            MessageBoxImage.None,
                            MessageBoxResult.None);
        }

        public static void ShowInfo(string text, string caption = "Информация")
        {
            MessageBox.Show(text,
                            caption,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information,
                            MessageBoxResult.None);
        }
    }
}
