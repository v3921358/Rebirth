#include <Windows.h>
#include <iostream>
#include <io.h>
#include "resource.h"

#define M_EXENAME "MapleStory.exe"
#define M_DLLNAME "RebirthClient.dll"

#define	ErrorBox(msg)	MessageBox(0, msg, 0, MB_ICONERROR)

BOOL IsElevated()
{
	BOOL fRet = FALSE;
	HANDLE hToken = NULL;
	HANDLE hProc = GetCurrentProcess();

	if (OpenProcessToken(hProc, TOKEN_QUERY, &hToken))
	{
		TOKEN_ELEVATION Elevation;
		DWORD cbSize = sizeof(TOKEN_ELEVATION);

		if (GetTokenInformation(hToken, TokenElevation, &Elevation, sizeof(Elevation), &cbSize))
		{
			fRet = Elevation.TokenIsElevated;
		}
	}

	if (hToken)
	{
		CloseHandle(hToken);
	}

	return fRet;
}

BOOL LaunchMaple()
{
	if (_access(M_DLLNAME, 0) == -1)
	{
		ErrorBox("Unable to find " M_DLLNAME);
		return FALSE;
	}

	if (_access(M_EXENAME, 0) == -1)
	{
		ErrorBox("Unable to find " M_EXENAME);
		return FALSE;
	}

	STARTUPINFO			MSStartUpInfo;
	PROCESS_INFORMATION	MSProcInfo;

	ZeroMemory(&MSStartUpInfo, sizeof(MSStartUpInfo));
	ZeroMemory(&MSProcInfo, sizeof(MSProcInfo));

	MSStartUpInfo.cb = sizeof(MSStartUpInfo);

	BOOL createRet = CreateProcess("MapleStory.exe", " GameLaunching",
		NULL, NULL, FALSE,
		CREATE_SUSPENDED,
		NULL, NULL, &MSStartUpInfo, &MSProcInfo);

	if (createRet)
	{
		HANDLE MSThreadHandle = MSProcInfo.hThread;
		HANDLE MSProcHandle = MSProcInfo.hProcess;

		const size_t LoadDllStrLen = strlen(M_DLLNAME);

		HMODULE KernelAddress = GetModuleHandle("Kernel32.dll");

		LPVOID LoadLibAddress = (LPVOID)GetProcAddress(KernelAddress, "LoadLibraryA");

		LPVOID RemoteString = (LPVOID)VirtualAllocEx(MSProcHandle, NULL, LoadDllStrLen, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

		WriteProcessMemory(MSProcHandle, (LPVOID)RemoteString, M_DLLNAME, LoadDllStrLen, NULL);

		CreateRemoteThread(MSProcHandle, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLibAddress, (LPVOID)RemoteString, NULL, NULL);

		Sleep(100);

		ResumeThread(MSThreadHandle);

		CloseHandle(MSThreadHandle);
		CloseHandle(MSProcHandle);

		return TRUE;
	}
	else
	{
		ErrorBox("Unable to CreateProcess");
		return FALSE;
	}
}

int CALLBACK WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	if (IsElevated())
		return LaunchMaple();
	else
		ErrorBox("Please run as administrator!");

	return 0;
};