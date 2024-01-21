using System.Windows;
using System.Windows.Media;

namespace ArtWiz.View.Utils
{
    public static class ViewExtension
    {
        public static T? FindAncestor<T>(this DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);

            return null;
        }
    }
}
