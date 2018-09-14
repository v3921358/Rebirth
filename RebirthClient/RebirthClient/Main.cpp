//---------------------------------------------------------------------------------------------
// v95 Localhost Enabler - Rajan
//---------------------------------------------------------------------------------------------

#include <WinSock2.h>
#include <WS2spi.h>
#include <Windows.h>
#include <winnt.h>
#include <stdio.h>
#include <intrin.h>
#include <dbghelp.h>
#include "Detours.h"
#include "NMCO\NMGeneral.h"
#include "NMCO\NMFunctionObject.h"
#include "NMCO\NMSerializable.h"

#include <winternl.h>

//Libraries
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "detours.lib")

//Macros
#define M_WINDOWNAME		"May Pul Uh Stoh Reeeeeeeeeee"
#define M_REPLACEPATTERN1	"63.251.217" //63.251.217.[1-4]
#define M_REPLACEPATTERN2	":8585"
#define M_HOSTNAME			"127.0.0.1"
#define M_MUTEXNAME			"meteora"
#define M_SPLASHURL			"https://mapleme.me/"
#define M_MSGBOXURL			"https://twitch.tv/rajan710"
#define M_MSGBOXMSG			"Login server is down... Watch my stream instead???"

#define M_HOOK(x,y)	{ if(!x(y)) MessageBoxA(0,"Failed to hook title", #x,0); }

//Etc
HMODULE			g_ehsvc;
char*			g_UserName = new char[PASSPORT_SIZE];
BOOL			g_CheckHotKeys = TRUE;

//Program Flags
const BOOL		g_SkipSplash = TRUE;
const BOOL		g_SkipAd = TRUE;
const DWORD		g_SkipSecurityClient = FALSE;

//Function Address'
const DWORD		g_HideDll = 0x0045EBD0;
const DWORD		g_TSingleton__CSecurityClient__IsInstantiated = 0x004AD020;

//---------------------------------------------------------------------------------------------
struct ZExceptionHandler
{
	void* m_cs;					//ZFatalSectionData m_cs;
	volatile int m_bInFilter;
	char m_sReportFileName[260];
	int(__stdcall *m_pPreviousFilter)(_EXCEPTION_POINTERS *);
	void *m_hReportFile;
	int m_bOverwrite;
	int m_bNoDialog;
	int m_bNoMiniDump;
	void(__cdecl *m_pfnHandler)(int, const char *);
	void *m_hProcess;
	int(__stdcall *_SymInitialize)(void *, char *, int);
	int(__stdcall *_SymCleanup)(void *);
	int(__stdcall *_StackWalk)(unsigned int, void *, void *, _tagSTACKFRAME *, void *, int(__stdcall *)(void *, unsigned int, void *, unsigned int, unsigned int *), void *(__stdcall *)(void *, unsigned int), unsigned int(__stdcall *)(void *, unsigned int), unsigned int(__stdcall *)(void *, void *, _tagADDRESS *));
	void *(__stdcall *_SymFunctionTableAccess)(void *, unsigned int);
	unsigned int(__stdcall *_SymGetModuleBase)(void *, unsigned int);
	int(__stdcall *_SymGetSymFromAddr)(void *, unsigned int, unsigned int *, _IMAGEHLP_SYMBOL *);
	int(__stdcall *_SymGetLineFromAddr)(void *, unsigned int, unsigned int *, _IMAGEHLP_LINE *);
	int(__stdcall *_MiniDumpWriteDump)(void *, unsigned int, void *, _MINIDUMP_TYPE, _MINIDUMP_EXCEPTION_INFORMATION *, _MINIDUMP_USER_STREAM_INFORMATION *, _MINIDUMP_CALLBACK_INFORMATION *);
};

//struct ZXString<char>
//{
//	char *_m_pStr;
//};

struct CMsgbox		// : TSingleton<CMsgbox>
{
	void* vfptr;	//CMsgboxVtbl *vfptr;
	char* m_sMsg;	//ZXString<char> m_sMsg;
	char* m_sLink;	//ZXString<char> m_sLink;
	char* m_sDesc;	//ZXString<char> m_sDesc;
	tagRECT m_rtMsg;
	tagRECT m_rtLink;
	int m_bLinkClicked;
	HICON__ *m_hCursorHand;
	HICON__ *m_hCursorArrow;
};

struct StartUpWndParam
{
	char URL[512];
	int width;
	int height;
	unsigned int timeout;
	HINSTANCE__ *hInst;
};

//struct CWvsAppVtbl
//{
//	void *(__thiscall *__vecDelDtor)(CWvsApp *this, unsigned int);
//};

struct CWvsApp// : TSingleton<CWvsApp>
{
	void* vfptr; //CWvsAppVtbl *vfptr;
	HWND__ *m_hWnd;
	int m_bPCOMInitialized;
	unsigned int m_dwMainThreadId;
	HHOOK__ *m_hHook;
	int m_bWin9x;
	int m_nOSVersion;
	int m_nOSMinorVersion;
	int m_nOSBuildNumber;

	char* m_sCSDVersion;//ZXString<char> m_sCSDVersion;

	int m_b64BitInfo;
	int m_tUpdateTime; //44 ?
	int m_bFirstUpdate;

	char* m_sCmdLine; //ZXString<char> m_sCmdLine;

	int m_nGameStartMode;
	int m_bAutoConnect;
	int m_bShowAdBalloon;
	int m_bExitByTitleEscape;

	HRESULT m_hrZExceptionCode;
	HRESULT m_hrComErrorCode;

	unsigned int m_dwSecurityErrorCode;

	int m_nTargetVersion;
	int m_tLastServerIPCheck;
	int m_tLastServerIPCheck2;
	int m_tLastGGHookingAPICheck;
	int m_tLastSecurityCheck;

	void *m_ahInput[3];

	int m_tNextSecurityCheck;

	bool m_bEnabledDX9;

	void* m_pBackupBuffer;//ZArray<unsigned char> m_pBackupBuffer;

	unsigned int m_dwBackupBufferSize;
	unsigned int m_dwClearStackLog;

	int m_bWindowActive;
};


//---------------------------------------------------------------------------------------------

typedef int (WINAPI* pWSPStartup)(WORD wVersionRequested, LPWSPDATA lpWSPData, LPWSAPROTOCOL_INFO lpProtocolInfo, WSPUPCALLTABLE UpcallTable, LPWSPPROC_TABLE lpProcTable);
typedef BOOL(WINAPI* pCreateProcessA)(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes, LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags, LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo, LPPROCESS_INFORMATION lpProcessInformation);
typedef BOOL(__cdecl* pNMCO_CallNMFunc)(int uFuncCode, BYTE* pCallingData, BYTE**ppReturnData, UINT32&	uReturnDataLen);

typedef HRESULT(WINAPI *pDirectInput8Create)(HINSTANCE, DWORD, REFIID, LPVOID*, LPUNKNOWN);

//---------------------------------------------------------------------------------------------

pWSPStartup				_WSPStartup;
pCreateProcessA			_CreateProcessA;
pNMCO_CallNMFunc		_NMCO_CallNMFunc;

WSPPROC_TABLE			procTable;
DWORD					nexonServer;
DWORD					userServer;

PTOP_LEVEL_EXCEPTION_FILTER	ZExceptionHandler__RawUnhandledExceptionFilter;
ZExceptionHandler* _ZExceptionHandler = nullptr;

CWvsApp* _CWvsApp = nullptr;

//Field::Init(char *this, int a2) is ruining my life btw

//---------------------------------------------------------------------------------------------

void Log(const char* format, ...)
{
	char buf[1024] = { 0 };

	va_list args;
	va_start(args, format);
	vsprintf(buf, format, args);

	//NOTE: We are hooking the OutputDebugStringA func used in this L0L
	OutputDebugStringA(buf);

	va_end(args);
}
void LogHexDump(char* ptr, int len)
{
	char buf[8192] = { 0 };
	char tmp[16] = { 0 };

	int bufPos = 0;

	for (int i = 0;i < len;i++)
	{
		auto value = (unsigned char)ptr[i];

		memset(tmp, 0, 16);
		sprintf(tmp, "%02X ", value);
		strcat(buf, tmp);
	}

	Log("[HexDump] %s", buf);
}
void MessageBoxFormat(const char* format, ...)
{
	char buf[1024] = { 0 };

	va_list args;
	va_start(args, format);
	vsnprintf(buf, 1023, format, args);

	MessageBoxA(NULL, buf, 0, 0);

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
void PatchClientSecurity()
{
	//TSingleton<CSecurityClient>::IsInstantiated(void)
	//33 C0					xor     eax, eax
	//39 05 30 8E C6 00		cmp     dword_C68E30, eax
	//0F 95 C0				setnz   al
	//C3					retn

	void* dst = (void*)(g_TSingleton__CSecurityClient__IsInstantiated);
	char code[] = { 0x33,0xC0,0x90,0x90,0x90,0x90,0x90,0x90 };
	memcpy(dst, code, 8);

	Log(__FUNCTION__);
}
void PatchHideDll()
{
	void* dst = (void*)(g_HideDll);
	char code[] = { 0xC3 }; //ret
	memcpy(dst, code, 1);

	Log(__FUNCTION__);
}
//---------------------------------------------------------------------------------------------

bool Hook_CreateWindowExA(bool bEnable)
{
	static auto _CreateWindowExA =
		decltype(&CreateWindowExA)(GetFuncAddress("USER32", "CreateWindowExA"));

	decltype(&CreateWindowExA) Hook = [](DWORD dwExStyle, LPCTSTR lpClassName, LPCTSTR lpWindowName, DWORD dwStyle, int x, int y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam) -> HWND
	{
		DWORD dwESI = NULL;
		_asm
		{
			MOV dwESI, ESI;
		}

		auto windowName = lpWindowName;
		auto ret = (DWORD)_ReturnAddress();

		Log("CreateWindowExA [%s] [%s] RET [%#08x]", lpClassName, lpWindowName, ret);

		if (!strcmp(lpClassName, "StartUpDlgClass"))
		{
			PatchHideDll();

			if (!g_SkipSecurityClient)
				PatchClientSecurity();

			if (!g_SkipSplash)
			{
				auto startParam = (StartUpWndParam*)dwESI;
				strcpy(startParam->URL, M_SPLASHURL);
			}
			else
			{
				return NULL;
			}
		}
		if (!strcmp(lpClassName, "NexonADBallon"))
		{
			if (g_SkipAd)
				return NULL;
		}
		else if (!strcmp(lpClassName, "MapleStoryClass"))
		{
			windowName = M_WINDOWNAME;

			_CWvsApp = (CWvsApp*)lpParam;
			Log("CWvsApp [%#08x]", lpParam);
		}

		return _CreateWindowExA(dwExStyle, lpClassName, windowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_CreateWindowExA), Hook);
}
bool Hook_DialogBoxParamA(bool bEnable)
{
	static auto _DialogBoxParamA =
		decltype(&DialogBoxParamA)(GetFuncAddress("USER32", "DialogBoxParamA"));

	decltype(&DialogBoxParamA) Hook = [](HINSTANCE hInstance, LPCSTR lpTemplateName, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM dwInitParam) -> INT_PTR
	{
		DWORD dwESI = NULL;

		_asm
		{
			MOV dwESI, ESI;
		}

		CMsgbox* msgBox = (CMsgbox*)dwESI;
		strcpy(msgBox->m_sMsg, M_MSGBOXMSG);
		strcpy(msgBox->m_sLink, M_MSGBOXURL);

		return _DialogBoxParamA(hInstance, lpTemplateName, hWndParent, lpDialogFunc, dwInitParam);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_DialogBoxParamA), Hook);
}

bool Hook_wvsprintfA(bool bEnable)
{
	static decltype(&wvsprintfA) _wvsprintf = wvsprintfA;

	decltype(&wvsprintfA) wvsprintf_Hook = [](LPSTR lpOutput, LPCSTR lpFmt, va_list arglist) -> int
	{
		auto ret = _wvsprintf(lpOutput, lpFmt, arglist);
		auto addy = (DWORD)_ReturnAddress();
		Log("[wvsprintfA] [%#08x] %s", addy, lpOutput);
		return ret;
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_wvsprintf), wvsprintf_Hook);
}
bool Hook_memset(bool bEnable)
{
	static decltype(&memset) _memset = memset;

	decltype(&memset) Hook = [](void *dest, int c, size_t count) -> void*
	{
		if (c == 0 && count == 72)
		{
			auto addy = (DWORD)_ReturnAddress();
			Log("[memset] [%#08x]", addy);
		}

		return _memset(dest, c, count);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_memset), Hook);
}

bool Hook_OutputDebugStringA(bool bEnable)
{
	static auto _OutputDebugStringA =
		decltype(&OutputDebugStringA)(GetFuncAddress("KERNEL32", "OutputDebugStringA"));

	decltype(&OutputDebugStringA) Hook = [](LPCTSTR lpOutputString) -> void
	{
		if (lpOutputString)
		{
			auto nStrLen = strlen(lpOutputString);

			if (nStrLen > 1) //This could cause trouble eventually
			{
				if (strstr(lpOutputString, "BrowserControlPane") || strstr(lpOutputString, "%u"))
				{
					return;
				}

				return _OutputDebugStringA(lpOutputString);
			}
		}
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_OutputDebugStringA), Hook);
}
bool Hook_SetUnhandledExceptionFilter(bool bEnable)
{
	static auto _SetUnhandledExceptionFilter =
		decltype(&SetUnhandledExceptionFilter)(GetFuncAddress("KERNEL32", "SetUnhandledExceptionFilter"));

	decltype(&SetUnhandledExceptionFilter) Hook = [](LPTOP_LEVEL_EXCEPTION_FILTER lpTopLevelExceptionFilter) -> LPTOP_LEVEL_EXCEPTION_FILTER
	{
		DWORD dwESI = NULL;

		_asm
		{
			MOV dwESI, ESI;
		}

		auto addy = (DWORD)_ReturnAddress();
		Log("[SetUnhandledExceptionFilter] ESI [%#08x] RET [%#08x]", dwESI, addy);

		//if (dwESI == 0xc391a8) //Outdated Addy
		//{
		//	ZExceptionHandler__RawUnhandledExceptionFilter = lpTopLevelExceptionFilter;
		//	_ZExceptionHandler = (ZExceptionHandler*)dwESI;
		//}

		return _SetUnhandledExceptionFilter(lpTopLevelExceptionFilter);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_SetUnhandledExceptionFilter), Hook);

}

bool Hook_GetProcAddress(bool bEnable)
{
	static auto _GetProcAddress =
		decltype(&GetProcAddress)(GetFuncAddress("KERNEL32", "GetProcAddress"));

	decltype(&GetProcAddress) Hook = [](HMODULE hModule, LPCSTR lpProcName) -> FARPROC
	{
		auto ret = _GetProcAddress(hModule, lpProcName);
		auto addy = (DWORD)_ReturnAddress();
		Log("[GetProcAddress] [%#08x] %s", addy, lpProcName);
		return ret;
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_GetProcAddress), Hook);
}
bool Hook_GetModuleFileNameW(bool bEnable)
{
	static auto _GetModuleFileNameW =
		decltype(&GetModuleFileNameW)(GetFuncAddress("KERNEL32", "GetModuleFileNameW"));

	decltype(&GetModuleFileNameW) Hook = [](HMODULE hModule, LPWSTR lpFilename, DWORD nSize) -> DWORD
	{
		//auto ret = _ReturnAddress();
		//Log("GetModuleFileNameW [%#08x] [hModule is NULL]", ret);
		return _GetModuleFileNameW(NULL, lpFilename, nSize);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_GetModuleFileNameW), Hook);
}

bool Hook_OpenMutexA(bool bEnable)
{
	static auto _OpenMutexA =
		decltype(&OpenMutexA)(GetFuncAddress("KERNEL32", "OpenMutexA"));

	decltype(&OpenMutexA) Hook = [](DWORD dwDesiredAccess, BOOL bInheritHandle, LPCSTR lpName) -> HANDLE
	{
		auto addy = (DWORD)_ReturnAddress();
		Log("[OpenMutexA] [%#08x] %s", addy, lpName);

		if (strstr(lpName, M_MUTEXNAME))
		{
			Log("[OpenMutexA] Faking Mutex %s", lpName);
			return (HANDLE)1;
		}

		return _OpenMutexA(dwDesiredAccess, bInheritHandle, lpName);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_OpenMutexA), Hook);
}

bool Hook_FreeLibrary(bool bEnable)
{
	static auto _FreeLibrary =
		decltype(&FreeLibrary)(GetFuncAddress("KERNEL32", "FreeLibrary"));

	decltype(&FreeLibrary) Hook = [](HMODULE hModule) -> BOOL
	{
		auto ret = (DWORD)_ReturnAddress();
		Log("[FreeLibrary] [%#08x]", ret);
		return _FreeLibrary(hModule);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_FreeLibrary), Hook);
}
bool Hook_LoadLibraryA(bool bEnable)
{
	static auto _LoadLibraryA =
		decltype(&LoadLibraryA)(GetFuncAddress("KERNEL32", "LoadLibraryA"));

	decltype(&LoadLibraryA) Hook = [](LPCTSTR lpFileName) -> HMODULE
	{
		auto ret = _LoadLibraryA(lpFileName);
		auto addy = (DWORD)_ReturnAddress();
		Log("[LoadLibraryA] [%#08x] %s", addy, lpFileName);

		return ret;
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_LoadLibraryA), Hook);

}
bool Hook_LoadLibraryExA(bool bEnable)
{
	static auto _LoadLibraryExA =
		decltype(&LoadLibraryExA)(GetFuncAddress("KERNEL32", "LoadLibraryExA"));

	decltype(&LoadLibraryExA) Hook = [](LPCSTR lpLibFileName, HANDLE hFile, DWORD  dwFlags) -> HMODULE
	{
		auto ret = (DWORD)_ReturnAddress();
		Log("LoadLibraryExA [%#08x] - %s %i", ret, lpLibFileName, dwFlags);
		return _LoadLibraryExA(lpLibFileName, hFile, dwFlags);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_LoadLibraryExA), Hook);
}

bool Hook_DirectInput8Create(bool bEnable)
{
	static auto _DirectInput8Create =
		(pDirectInput8Create)(GetFuncAddress("DINPUT8", "DirectInput8Create"));

	pDirectInput8Create Hook = [](HINSTANCE hinst, DWORD dwVersion, REFIID riidltf, LPVOID * ppvOut, LPUNKNOWN punkOuter) -> HRESULT
	{
		auto addy = (DWORD)_ReturnAddress();
		auto delay = 5000;

		Log("[DirectInput8Create] [%#08x] Sleep %d", addy, delay);
		Sleep(delay);

		return _DirectInput8Create(hinst, dwVersion, riidltf, ppvOut, punkOuter);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_DirectInput8Create), Hook);
}

bool Hook_closesocket(bool bEnable)
{
	static auto _closesocket =
		decltype(&closesocket)(GetFuncAddress("WS2_32", "closesocket"));

	decltype(&closesocket) Hook = [](SOCKET s) -> int
	{
		auto addy = (DWORD)_ReturnAddress();
		Log("[closesocket] [%#08x]", addy);
		return _closesocket(s);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_closesocket), Hook);
}

bool Hook_RegisterClassA(bool bEnable)
{
	static auto _RegisterClassA =
		decltype(&RegisterClassA)(GetFuncAddress("USER32", "RegisterClassA"));

	decltype(&RegisterClassA) Hook = [](CONST WNDCLASSA *lpWndClass) -> ATOM
	{
		auto addy = (DWORD)_ReturnAddress();
		Log("[RegisterClassA] [%#08x] %s", addy, lpWndClass->lpszMenuName);
		return _RegisterClassA(lpWndClass);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_RegisterClassA), Hook);
}

bool Hook_GetTickCount(bool bEnable)
{
	static auto _GetTickCount =
		decltype(&GetTickCount)(GetFuncAddress("KERNEL32", "GetTickCount"));

	decltype(&GetTickCount) Hook = []() -> DWORD
	{
		//auto addy = (DWORD)_ReturnAddress();
		//Log("[GetTickCount] [%#08x]", addy);
		return _GetTickCount();
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_GetTickCount), Hook);
}

bool Hook_NtCurrentTeb(bool bEnable)
{
	static decltype(&NtCurrentTeb) _NtCurrentTeb = NtCurrentTeb;

	decltype(&NtCurrentTeb) Hook = []() -> _TEB *
	{
		auto addy = (DWORD)_ReturnAddress();
		Log("[NtCurrentTeb] [%#08x]", addy);
		return _NtCurrentTeb();
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_NtCurrentTeb), Hook);
}

bool Hook_VirtualQuery(bool bEnable)
{
	static decltype(&VirtualQuery) _VirtualQuery = VirtualQuery;

	decltype(&VirtualQuery) Hook = [](LPCVOID lpAddress, PMEMORY_BASIC_INFORMATION lpBuffer, SIZE_T dwLength) -> SIZE_T
	{
		auto addy = (DWORD)_ReturnAddress();
		Log("[VirtualQuery] [%#08x]", addy);
		return _VirtualQuery(lpAddress, lpBuffer, dwLength);
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_VirtualQuery), Hook);
}

bool Hook_IsDebuggerPresent(bool bEnable)
{
	static decltype(&IsDebuggerPresent) _IsDebuggerPresent = IsDebuggerPresent;

	decltype(&IsDebuggerPresent) Hook = []() -> BOOL
	{
		auto addy = (DWORD)_ReturnAddress();
		Log("[IsDebuggerPresent] [%#08x]", addy);
		return FALSE;
	};

	return SetHook(bEnable, reinterpret_cast<void**>(&_IsDebuggerPresent), Hook);
}

//---------------------------------------------------------------------------------------------

int WINAPI WSPGetPeerName_Hook(SOCKET s, struct sockaddr *name, LPINT namelen, LPINT lpErrno)
{
	int ret = procTable.lpWSPGetPeerName(s, name, namelen, lpErrno);

	char buf[50];
	DWORD len = 50;
	WSAAddressToString((sockaddr*)name, *namelen, NULL, buf, &len);
	Log("[GetPeerName] Original: %s", buf);

	sockaddr_in* service = (sockaddr_in*)name;
	memcpy(&service->sin_addr, &nexonServer, sizeof(DWORD));

	return  ret;
}
int WINAPI WSPConnect_Hook(SOCKET s, const struct sockaddr *name, int namelen, LPWSABUF lpCallerData, LPWSABUF lpCalleeData, LPQOS lpSQOS, LPQOS lpGQOS, LPINT lpErrno)
{
	char buf[50];
	DWORD len = 50;
	WSAAddressToString((sockaddr*)name, namelen, NULL, buf, &len);
	Log("[WSPConnect] Original: %s", buf);

	if (strstr(buf, M_REPLACEPATTERN1))
	{
		sockaddr_in* service = (sockaddr_in*)name;
		memcpy(&nexonServer, &service->sin_addr, sizeof(DWORD)); //sin_adder -> nexonServer
		service->sin_addr.S_un.S_addr = inet_addr(M_HOSTNAME);
	}

	if (strstr(buf, M_REPLACEPATTERN2))
	{
		//g_CheckHotKeys = FALSE;
	}

	return procTable.lpWSPConnect(s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS, lpErrno);
}
int WINAPI WSPStartup_Hook(WORD wVersionRequested, LPWSPDATA lpWSPData, LPWSAPROTOCOL_INFO lpProtocolInfo, WSPUPCALLTABLE UpcallTable, LPWSPPROC_TABLE lpProcTable)
{
	Log("[WSPStartup] Hijacked the winsock table");

	int ret = _WSPStartup(wVersionRequested, lpWSPData, lpProtocolInfo, UpcallTable, lpProcTable);
	procTable = *lpProcTable;

	lpProcTable->lpWSPConnect = WSPConnect_Hook;
	lpProcTable->lpWSPGetPeerName = WSPGetPeerName_Hook;

	return ret;
}
BOOL WINAPI CreateProcessA_Hook(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes, LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags, LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo, LPPROCESS_INFORMATION lpProcessInformation)
{
	MessageBox(0, lpApplicationName, lpCommandLine, 0);
	Log("CreateProcessA: %s - %s", lpApplicationName, lpCommandLine);
	return _CreateProcessA(lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, lpProcessInformation);
}

BOOL NMCO_CallNMFunc_Hook(int uFuncCode, BYTE* pCallingData, BYTE**ppReturnData, UINT32& uReturnDataLen)
{
	Log("[NMCO_CallNMFunc_Hook] uFuncCode %d", uFuncCode);

	//CWvsApp::InitializeAuth
	if (uFuncCode == kNMFuncCode_SetLocale || uFuncCode == kNMFuncCode_Initialize)
	{
		CNMSimpleStream* returnStream = new CNMSimpleStream(); // Memleaked actually. 
		CNMSetLocaleFunc* retFunc = new CNMSetLocaleFunc(); // Memleaked actually. 
		retFunc->SetReturn();
		retFunc->bSuccess = true;

		if (retFunc->Serialize(*returnStream) == false)
			MessageBoxA(NULL, "Could not Serialize?!", 0, 0);

		*ppReturnData = returnStream->GetBufferPtr();
		uReturnDataLen = returnStream->GetBufferSize();

		return TRUE;
	}
	else if (uFuncCode == kNMFuncCode_LoginAuth)
	{
		CNMSimpleStream	ssStream;
		ssStream.SetBuffer(pCallingData);

		CNMLoginAuthFunc pFunc;
		pFunc.SetCalling();
		pFunc.DeSerialize(ssStream);

		memcpy(g_UserName, pFunc.szNexonID, PASSPORT_SIZE);
		Log("[CNMLoginAuthFunc] Username: %s", g_UserName);

		// Return to the client that login was successful.. NOT
		CNMSimpleStream* returnStream = new CNMSimpleStream(); // Memleaked actually. 
		CNMLoginAuthFunc* retFunc = new CNMLoginAuthFunc(); // Memleaked actually. 
		retFunc->SetReturn();
		retFunc->nErrorCode = kLoginAuth_OK;
		retFunc->bSuccess = true;

		if (retFunc->Serialize(*returnStream) == false)
			MessageBoxA(NULL, "Could not Serialize?!", 0, 0);

		*ppReturnData = returnStream->GetBufferPtr();
		uReturnDataLen = returnStream->GetBufferSize();

		return TRUE;
	}
	else if (uFuncCode == kNMFuncCode_GetNexonPassport)
	{
		CNMSimpleStream* ssStream = new CNMSimpleStream(); // Memleaked actually. 

		CNMGetNexonPassportFunc* pFunc = new CNMGetNexonPassportFunc(); // Memleaked actually. 
		pFunc->bSuccess = true;

		strcpy(pFunc->szNexonPassport, g_UserName);

		pFunc->SetReturn();

		if (pFunc->Serialize(*ssStream) == false)
			MessageBoxA(NULL, "Could not Serialize?!", 0, 0);

		*ppReturnData = ssStream->GetBufferPtr();
		uReturnDataLen = ssStream->GetBufferSize();

		return TRUE;
	}
	else if (uFuncCode == kNMFuncCode_LogoutAuth)
	{
		return TRUE;
	}

	Log("Woops. Missing something: %x", uFuncCode);

	return _NMCO_CallNMFunc(uFuncCode, pCallingData, ppReturnData, uReturnDataLen);
}

//---------------------------------------------------------------------------------------------
BOOL Hook_NMCO()
{
	auto address = GetFuncAddress("nmcogame", "NMCO_CallNMFunc");

	if (!address)
		return FALSE;

	_NMCO_CallNMFunc = (pNMCO_CallNMFunc)address;

	return SetHook(true, (PVOID*)&_NMCO_CallNMFunc, (PVOID)NMCO_CallNMFunc_Hook);
}
BOOL Hook_Winsock()
{
	auto address = GetFuncAddress("MSWSOCK", "WSPStartup");

	if (!address)
		return FALSE;

	_WSPStartup = (pWSPStartup)address;

	return SetHook(true, (PVOID*)&_WSPStartup, (PVOID)WSPStartup_Hook);
}
BOOL Hook_CreateProcessA()
{
	auto address = GetFuncAddress("KERNEL32", "CreateProcessA");

	if (!address)
		return FALSE;

	_CreateProcessA = (pCreateProcessA)address;

	return SetHook(true, (PVOID*)&_CreateProcessA, (PVOID)CreateProcessA_Hook);
}
//---------------------------------------------------------------------------------------------
DWORD WINAPI HotKeyThread()
{
	Log("[HotKeyThread] Started");
	while (g_CheckHotKeys)
	{
		if (GetAsyncKeyState(VK_F3))
		{
			if (_CWvsApp)
			{
				LogHexDump((char*)_CWvsApp, sizeof(CWvsApp));

				Log("CWvsApp m_dwMainThreadId %d", _CWvsApp->m_dwMainThreadId);

				Log("CWvsApp m_tUpdateTime %d", _CWvsApp->m_tUpdateTime);
				Log("CWvsApp m_tLastServerIPCheck %d", _CWvsApp->m_tLastServerIPCheck);
				Log("CWvsApp m_tLastServerIPCheck2 %d", _CWvsApp->m_tLastServerIPCheck2);
				Log("CWvsApp m_tLastGGHookingAPICheck %d", _CWvsApp->m_tLastGGHookingAPICheck);
				Log("CWvsApp m_tLastSecurityCheck %d", _CWvsApp->m_tLastSecurityCheck);
				Log("CWvsApp m_tNextSecurityCheck %d", _CWvsApp->m_tNextSecurityCheck);
			}
		}

		Sleep(100);
	}
	Log("[HotKeyThread] Ended");
	return 1;
}
DWORD WINAPI CWvsAppThread()
{
	Log("[CWvsAppThread] Started");
	while (g_CheckHotKeys)
	{
		if (_CWvsApp && _CWvsApp->m_tUpdateTime)
		{
			Log("CWvsApp m_tUpdateTime PATCH %d", _CWvsApp->m_tUpdateTime);

			_CWvsApp->m_tLastServerIPCheck = _CWvsApp->m_tUpdateTime;
			_CWvsApp->m_tLastServerIPCheck2 = _CWvsApp->m_tUpdateTime;
			_CWvsApp->m_tLastGGHookingAPICheck = _CWvsApp->m_tUpdateTime;
			_CWvsApp->m_tLastSecurityCheck = _CWvsApp->m_tUpdateTime;
			_CWvsApp->m_tNextSecurityCheck = _CWvsApp->m_tUpdateTime;

		}
		Sleep(5000);
	}
	Log("[CWvsAppThread] Ended");
	return 1;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	DisableThreadLibraryCalls(hinstDLL);

	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		Log("Injected into MapleStory PID: %i", GetCurrentProcessId());

		CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)&HotKeyThread, NULL, NULL, NULL);
		CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)&CWvsAppThread, NULL, NULL, NULL);

		g_ehsvc = LoadLibraryA("HShield\\ehsvc.dll");

		if (!g_ehsvc)
			MessageBoxA(0, "Failed loading ehsvc.dll", 0, 0);

		if (!Hook_NMCO())
			MessageBoxA(0, "Failed Hook_NMCO", 0, 0);

		if (!Hook_Winsock())
			MessageBoxA(0, "Failed Hook_Winsock", 0, 0);

		if (!Hook_CreateProcessA())
			MessageBoxA(0, "Failed Hook_CreateProcessA", 0, 0);

			M_HOOK(Hook_CreateWindowExA, true)
			M_HOOK(Hook_DialogBoxParamA, true)
			M_HOOK(Hook_wvsprintfA, true)
			//M_HOOK(Hook_memset, true) //OH DONT DO IT (Seriously)
			M_HOOK(Hook_OutputDebugStringA, true)
			//M_HOOK(Hook_SetUnhandledExceptionFilter, true)
			//M_HOOK(Hook_GetProcAddress, true)
			M_HOOK(Hook_GetModuleFileNameW, true)
			M_HOOK(Hook_OpenMutexA, true)
			//M_HOOK(Hook_FreeLibrary, true)
			//M_HOOK(Hook_LoadLibraryA, true)
			//M_HOOK(Hook_LoadLibraryExA, true)
			//M_HOOK(Hook_DirectInput8Create, true)
			//M_HOOK(Hook_closesocket, true)
			//M_HOOK(Hook_RegisterClassA, true)
			//M_HOOK(Hook_GetTickCount, true)
			//M_HOOK(Hook_NtCurrentTeb, true)
			//M_HOOK(Hook_VirtualQuery,true)
			//M_HOOK(Hook_IsDebuggerPresent,true)
	}
	else if (fdwReason == DLL_PROCESS_DETACH)
	{
		g_CheckHotKeys = FALSE;
	}
	return TRUE;
}