using MSHC.Values;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace StandaloneOrganizr.WPF
{
	public static class InterceptKeys
	{
		private const int MAX_DELAY = 300;

		private static readonly LowLevelKeyboardProc _proc = HookCallback;
		private static IntPtr _hookID = IntPtr.Zero;

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		private static int lastPartialTrigger = Int32.MinValue;

		public static Action OnHotkey = () => { };

		public static void Start()
		{
			_hookID = SetHook(_proc);
		}

		public static void Stop()
		{
			UnhookWindowsHookEx(_hookID);
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WindowsHooks.WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0) return CallNextHookEx(_hookID, nCode, wParam, lParam);

			bool isUp = (wParam == (IntPtr) WindowsMessages.WM_KEYUP || wParam == (IntPtr) WindowsMessages.WM_SYSKEYUP);

			if (!isUp) return CallNextHookEx(_hookID, nCode, wParam, lParam);

			if (Marshal.ReadInt32(lParam) == VirtualKeyCodes.VK_SPACE && Keyboard.IsKeyDown(Key.LeftAlt))
			{
				if (lastPartialTrigger > 0 && Environment.TickCount - lastPartialTrigger < MAX_DELAY)
				{
					OnHotkey();
					lastPartialTrigger = -1;
				}
				else
				{
					lastPartialTrigger = Environment.TickCount;
				}

			}
			else if (Marshal.ReadInt32(lParam) == VirtualKeyCodes.VK_SPACE && !Keyboard.IsKeyDown(Key.LeftAlt))
			{
				if (lastPartialTrigger > 0 && Environment.TickCount - lastPartialTrigger < MAX_DELAY)
				{
					OnHotkey();
				}

				lastPartialTrigger = -1;
			}

			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}
