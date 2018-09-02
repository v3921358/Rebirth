//---------------------------------------------------------------------------------------------
// v95 Localhost Enabler - Rajan
//---------------------------------------------------------------------------------------------
#include <WinSock2.h>
#include <WS2spi.h>
#include <Windows.h>
#include <stdio.h>
#include <intrin.h>
#include "Detours.h"
//---------------------------------------------------------------------------------------------
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "detours.lib")
//---------------------------------------------------------------------------------------------
#define M_WINDOWNAME		"Rebirth v95"
#define M_REPLACEPATTERN	":8484"			//63.251.217.1 - 4
#define M_HOSTNAME			"127.0.0.1"
//---------------------------------------------------------------------------------------------
typedef BOOL(WINAPI* pCreateProcessA)(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes, LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags, LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo, LPPROCESS_INFORMATION lpProcessInformation);
typedef DWORD(WINAPI* pGetModuleFileNameW)(HMODULE hModule, LPTSTR lpFilename, DWORD nSize);
typedef HWND(WINAPI* pCreateWindowExA)(DWORD dwExStyle, LPCTSTR lpClassName, LPCTSTR lpWindowName, DWORD dwStyle, int x, int y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam);
typedef int (WINAPI* pWSPStartup)(WORD wVersionRequested, LPWSPDATA lpWSPData, LPWSAPROTOCOL_INFO lpProtocolInfo, WSPUPCALLTABLE UpcallTable, LPWSPPROC_TABLE lpProcTable);

typedef void*(__cdecl* pmemset)(void *dest, int c, size_t count);


//---------------------------------------------------------------------------------------------
pCreateProcessA		_CreateProcessA;
pGetModuleFileNameW	_GetModuleFileNameW;
pCreateWindowExA	_CreateWindowExA;
pWSPStartup			_WSPStartup;
pmemset				_memset;

WSPPROC_TABLE procTable;

DWORD nexonServer;
DWORD userServer;
//---------------------------------------------------------------------------------------------
void Log2(const char *fmt, ...)
{
	va_list list;
	static char buffer[1024];
	static DWORD w;

	va_start(list, fmt);

	// to console
	DWORD len = sprintf(buffer, fmt, list);
	WriteConsoleA(GetStdHandle(STD_OUTPUT_HANDLE), buffer, len, (DWORD *)&w, NULL);

	va_end(list);
}
void Log(const char* format, ...)
{
	char buf[1024] = { 0 };

	va_list args;
	va_start(args, format);
	vsprintf(buf, format, args);

	OutputDebugString(buf);

	va_end(args);
}
BOOL SetHook(BOOL bInstall, PVOID* ppvTarget, PVOID pvDetour)
{
	if (DetourTransactionBegin() != NO_ERROR)
	{
		return FALSE;
	}

	auto tid = GetCurrentThread();

	if (DetourUpdateThread(tid) == NO_ERROR)
	{
		auto func = bInstall ? DetourAttach : DetourDetach;

		if (func(ppvTarget, pvDetour) == NO_ERROR)
		{
			if (DetourTransactionCommit() == NO_ERROR)
			{
				return TRUE;
			}
		}
	}

	DetourTransactionAbort();
	return FALSE;
}
DWORD GetFuncAddress(LPCSTR lpModule, LPCSTR lpFunc)
{
	auto mod = LoadLibraryA(lpModule);

	if (!mod)
	{
		return 0;
	}

	auto address = (DWORD)GetProcAddress(mod, lpFunc);

	Log(__FUNCTION__ " [%s] %s @ %8X", lpModule, lpFunc, address);

	return (DWORD)GetProcAddress(mod, lpFunc);
}
unsigned int FindAoB(unsigned int start, unsigned int end, unsigned char *pattern, unsigned int length, unsigned char wildcard)
{
	bool found = false;

	for (unsigned int i = start; i < end - length; i++)
	{
		for (unsigned int j = 0; j < length; j++)
		{
			if (pattern[j] == wildcard) continue;
			if (*(unsigned char *)(i + j) != pattern[j])
			{
				found = false;
				break;
			}

			found = true;
		}

		if (found) return i;
	}

	return 0;
}
//---------------------------------------------------------------------------------------------
BOOL WINAPI CreateProcessA_detour(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes, LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags, LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo, LPPROCESS_INFORMATION lpProcessInformation)
{
	Log("CreateProcessA: %s - %s", lpApplicationName, lpCommandLine);
	return _CreateProcessA(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation);
}
DWORD WINAPI GetModuleFileNameW_detour(HMODULE hModule, LPTSTR lpFilename, DWORD nSize)
{
	Log("GetModuleFileNameW");
	return _GetModuleFileNameW(NULL, lpFilename, nSize);
}
int WINAPI WSPGetPeerName_detour(SOCKET s, struct sockaddr *name, LPINT namelen, LPINT lpErrno)
{
	int ret = procTable.lpWSPGetPeerName(s, name, namelen, lpErrno);

	char buf[50];
	DWORD len = 50;
	WSAAddressToString((sockaddr*)name, *namelen, NULL, buf, &len);
	Log("GetPeerName Original: %s", buf);

	//sockaddr_in* service = (sockaddr_in*)name;
	//memcpy(&service->sin_addr, &nexonServer, sizeof(DWORD));

	return  ret;
}
int WINAPI WSPConnect_detour(SOCKET s, const struct sockaddr *name, int namelen, LPWSABUF lpCallerData, LPWSABUF lpCalleeData, LPQOS lpSQOS, LPQOS lpGQOS, LPINT lpErrno)
{
	char buf[50];
	DWORD len = 50;
	WSAAddressToString((sockaddr*)name, namelen, NULL, buf, &len);
	Log("WSPConnect Original: %s", buf);

	//if (strstr(buf, M_REPLACEPATTERN))
	//{
	//	sockaddr_in* service = (sockaddr_in*)name;
	//	memcpy(&nexonServer, &service->sin_addr, sizeof(DWORD)); //sin_adder -> nexonServer
	//	service->sin_addr.S_un.S_addr = inet_addr(M_HOSTNAME);
	//}

	return procTable.lpWSPConnect(s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS, lpErrno);
}
int WINAPI WSPStartup_detour(WORD wVersionRequested, LPWSPDATA lpWSPData, LPWSAPROTOCOL_INFO lpProtocolInfo, WSPUPCALLTABLE UpcallTable, LPWSPPROC_TABLE lpProcTable)
{
	Log("Hijacked the winsock proc table");

	int ret = _WSPStartup(wVersionRequested, lpWSPData, lpProtocolInfo, UpcallTable, lpProcTable);
	procTable = *lpProcTable;

	lpProcTable->lpWSPConnect = WSPConnect_detour;
	lpProcTable->lpWSPGetPeerName = WSPGetPeerName_detour;

	return ret;
}
HWND WINAPI CreateWindowExA_detour(DWORD dwExStyle, LPCTSTR lpClassName, LPCTSTR lpWindowName, DWORD dwStyle, int x, int y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam)
{
	auto windowName = lpWindowName;

	if (!strcmp(lpClassName, "StartUpDlgClass") || !strcmp(lpClassName, "NexonADBallon"))
	{
		return NULL;
	}
	//else if (!strcmp(lpClassName, "MapleStoryClass"))
	//{
		//windowName = M_WINDOWNAME;
	//}

	return _CreateWindowExA(dwExStyle, lpClassName, windowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
}
void* __cdecl memset_hook(void *dest, int c, size_t count)
{
	auto ret = (DWORD)_ReturnAddress();

	if (count == 72 && c == 0) {

		//Log("memset %i %8x", count, ret);

		//if(ret == 0x52ca6979)
		//	return dest;
	}
	return _memset(dest, c, count);
}


//---------------------------------------------------------------------------------------------
BOOL HookWinsock()
{
	auto address = GetFuncAddress("MSWSOCK", "WSPStartup");

	if (!address)
		return FALSE;

	_WSPStartup = (pWSPStartup)address;

	return SetHook(true, (PVOID*)&_WSPStartup, (PVOID)WSPStartup_detour);
}
BOOL HookWindow()
{
	auto address = GetFuncAddress("USER32", "CreateWindowExA");

	if (!address)
		return FALSE;

	_CreateWindowExA = (pCreateWindowExA)address;

	return SetHook(true, (PVOID*)&_CreateWindowExA, (PVOID)CreateWindowExA_detour);
}
BOOL HookHackShield()
{
	auto address = GetFuncAddress("KERNEL32", "CreateProcessA");

	if (!address)
		return FALSE;

	_CreateProcessA = (pCreateProcessA)address;

	return SetHook(true, (PVOID*)&_CreateProcessA, (PVOID)CreateProcessA_detour);
}
BOOL HookWin10Fix()
{
	auto address = GetFuncAddress("KERNEL32", "GetModuleFileNameW");

	if (!address)
		return FALSE;

	_GetModuleFileNameW = (pGetModuleFileNameW)address;

	return SetHook(true, (PVOID*)&_GetModuleFileNameW, (PVOID)GetModuleFileNameW_detour);
}
BOOL HookHideDll()
{
	auto address = GetFuncAddress("MSVCRT", "memset");

	if (!address)
		return FALSE;

	_memset = (pmemset)address;

	return SetHook(true, (PVOID*)&_memset, (PVOID)memset_hook);
}

//---------------------------------------------------------------------------------------------
BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	DisableThreadLibraryCalls(hinstDLL);

	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		Log("Injected into MapleStory PID: %i", GetCurrentProcessId());

		bool v1 = HookWinsock();

		//if (!v1)
		//	MessageBoxA(0, "Failed Winsock Hook", 0, 0);

		bool v2 = HookWindow();

		//if (!v2)
		//	MessageBoxA(0, "Failed Window Hook", 0, 0);

		//bool v3 = HookHackShield();

		//if (!v3)
		//	MessageBoxA(0, "Failed HackShield Hook", 0, 0);

		bool v4 = HookWin10Fix();

		//if (!v4)
		//	MessageBoxA(0, "Failed Win10Fix Hook", 0, 0);

		//bool v5 = HookHideDll();

		//if (!v5)
		//	MessageBoxA(0, "Failed HideDll Hook", 0, 0);

		//return v1 && v2 && v3 && v4 && v5;
	}
	return TRUE;
	//	return FALSE;
}