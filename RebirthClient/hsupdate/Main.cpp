#include <windows.h>

int __stdcall WinMain(HINSTANCE, HINSTANCE, LPSTR, int)
{
	HANDLE event = CreateEvent(NULL, TRUE, FALSE, "Global\\EF81BA4B-4163-44f5-90E2-F05C1E49C12D");
	SetEvent(event);
	CloseHandle(event);

	return 0;
}