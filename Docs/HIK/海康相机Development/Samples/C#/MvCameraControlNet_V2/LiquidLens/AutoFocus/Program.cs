/*
 * 这个示例演示SDK的自动对焦功能，包括枚举、连接相机、配置液体透镜参数、执行自动对焦、取图等功能。
 * This program shows the auto-focus functions of SDK, including enumeration, camera connection, liquid lens parameter configuration, auto-focus execution, image grabbing, and image saving.
 */

using System;
using System.Windows.Forms;

namespace AutoFocusDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
