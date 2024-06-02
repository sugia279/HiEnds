using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiEndsCore.Helper
{
    public static class ArrayExtensions
    {
        public static void MoveItemUp<T>(this T[] array, int selectedIndex)
        {
            if (selectedIndex > 0 && selectedIndex < array.Length)
            {
                T temp = array[selectedIndex];
                array[selectedIndex] = array[selectedIndex - 1];
                array[selectedIndex - 1] = temp;
            }
        }

        public static void MoveItemDown<T>(this T[] array, int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < array.Length - 1)
            {
                T temp = array[selectedIndex];
                array[selectedIndex] = array[selectedIndex + 1];
                array[selectedIndex + 1] = temp;
            }
        }
    }
}
