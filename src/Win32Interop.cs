using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public delegate IntPtr SUBCLASSPROC(
	IntPtr hwnd,
	uint uMsg,
	IntPtr wParam,
	IntPtr lParam,
	IntPtr uIdSubclass,
	uint dwRefData
);
