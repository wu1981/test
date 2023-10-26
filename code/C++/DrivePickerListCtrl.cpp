/////////////////////////////////////////////////////////////////////////////
//
// DrivePickerListCtrl.cpp : implementation file for the drive selection
//                           list control.
//
/////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "DrivePickerListCtrl.h"
#include "ToolClass.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
// CDrivePickerListCtrl

CDrivePickerListCtrl::CDrivePickerListCtrl()
{
}

CDrivePickerListCtrl::~CDrivePickerListCtrl()
{
    if ( NULL != m_ImgList.GetSafeHandle() )
        {
        m_ImgList.Detach();
        }
}


BEGIN_MESSAGE_MAP(CDrivePickerListCtrl, CListCtrl)
	//{{AFX_MSG_MAP(CDrivePickerListCtrl)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// CDrivePickerListCtrl public functions


//////////////////////////////////////////////////////////////////////////
//
// Function:    InitList
//
// Description:
//  Does all the initialization of the list control, sets up the styles,
//  and fills in the list with drives that match the flags you specify.
//
// Input:
//  nIconSize: [in] The size of the icons to show in the control.  The sizes
//             supported currently are 16 (for 16x16) and 32 (for 32x32).
//  nFlags: [in] Flags that determine which type of drives will appear in
//          the list.  See the DDS_DLIL_* constants in the header file.
//
// Returns:
//  Nothing.
//
//////////////////////////////////////////////////////////////////////////

void CDrivePickerListCtrl::InitList ( int nIconSize, DWORD dwFlags )
{
    // Only 16x16 and 32x32 sizes are supported now.
    ASSERT ( 16 == nIconSize  ||  32 == nIconSize );
    // Check that only valid flags were passed in.
    ASSERT ( ( dwFlags & DDS_DLIL_ALL_DRIVES ) == dwFlags );


    CommonInit ( 32 == nIconSize, dwFlags );
}


//////////////////////////////////////////////////////////////////////////
//
// Function:    SetSelection
//
// Description:
//  Checks the specified drives in the control.
//
// Input:
//  dwDrives: [in] Flags indicating which drives should be checked.  The
//            flags are bit 0 for A:, bit 1 for B:, etc.
//  -or-
//  szDriveList: [in] String containing the drive letters to select, for 
//               example, "ACD".  The string must contain only letters, but
//               they can be upper- or lower-case.
//
// Returns:
//  Nothing.
//
//////////////////////////////////////////////////////////////////////////

void CDrivePickerListCtrl::SetSelection ( const DWORD dwDrives )
{
int     nIndex;
int     nMaxIndex = GetItemCount() - 1;
CString sDrives;
TCHAR   cNextDrive;
BOOL    bCheck;

    // Verify that the only bits set are bits 0 thru 25.
    ASSERT ( (dwDrives &  0x3FFFFFF) == dwDrives );

    for ( nIndex = 0; nIndex <= nMaxIndex; nIndex++ )
        {
        cNextDrive = static_cast<TCHAR>( GetItemData ( nIndex ));

        // Internal check - letters are always kept as uppercase.
        ASSERT ( _istupper ( cNextDrive ));

        bCheck = ( dwDrives & (1 << (cNextDrive - 'A')) );

        SetCheck ( nIndex, bCheck );
        }
}

void CDrivePickerListCtrl::SetSelection ( LPCTSTR szDrives )
{
int     nIndex;
int     nMaxIndex = GetItemCount() - 1;
CString sDrives;
TCHAR   cNextDrive;
BOOL    bCheck;

#ifdef _DEBUG
LPCTSTR pchNextDrive = szDrives;

    ASSERT ( AfxIsValidString ( szDrives ));

    // Verify that only letters a thru z are in szDrives.

    for ( ; '\0' != *pchNextDrive; pchNextDrive++ )
        {
        ASSERT ( _totupper ( *pchNextDrive ) >= 'A' &&
                 _totupper ( *pchNextDrive ) <= 'Z' );
        }
#endif

    // Make a local copy of the drives string that we can mess with - we 
    // need the drive letters to be all uppercase.

    sDrives = szDrives;
    sDrives.MakeUpper();

    // For each drive currently in the list, if its letter is in sDrives,
    // check the checkbox.  If it is not in sDrives, then uncheck the box.

    for ( nIndex = 0; nIndex <= nMaxIndex; nIndex++ )
        {
        cNextDrive = static_cast<TCHAR>( GetItemData ( nIndex ));

        // Internal check - letters are always kept as uppercase.
        ASSERT ( _istupper ( cNextDrive ));

        bCheck = ( -1 != sDrives.Find ( cNextDrive ));

        SetCheck ( nIndex, bCheck );
        }
}


//////////////////////////////////////////////////////////////////////////
//
// Function:    GetNumSelectedDrives
//
// Description:
//  Returns the number of drives whose checkboxes are checked.
//
// Input:
//  Nothing.
//
// Returns:
//  The number of drives that are checked.
//
//////////////////////////////////////////////////////////////////////////

BYTE CDrivePickerListCtrl::GetNumSelectedDrives() const
{
BYTE byNumDrives = 0;
int  nIndex;
int  nMaxIndex = GetItemCount() - 1;

    for ( nIndex = 0; nIndex <= nMaxIndex; nIndex++ )
        {
        if ( GetCheck ( nIndex ) )
            {
            byNumDrives++;
            }
        }

    return byNumDrives;
}


//////////////////////////////////////////////////////////////////////////
//
// Function:    GetSelectedDrives
//
// Description:
//  Returns the drives in the list that are checked.
//
// Input:
//  pdwSelectedDrives: [out] Pointer to a DWORD that is set to indicate the
//                     selected drives, where bit 0 corresponds to A:, bit 1
//                     corresponds to B:, etc.
//  -or-
//  szSelectedDrives: [out] Pointer to a buffer that is filled with a zero-
//                    terminated string where each characters is a selected
//                    drive, e.g., "ACDIX".  This buffer must be at least
//                    27 characters long.
//
// Returns:
//  Nothing.
//
//////////////////////////////////////////////////////////////////////////

void CDrivePickerListCtrl::GetSelectedDrives ( DWORD* pdwSelectedDrives ) const
{
int nIndex;
int nMaxIndex = GetItemCount() - 1;
TCHAR chDrive;

    ASSERT ( AfxIsValidAddress ( pdwSelectedDrives, sizeof(DWORD) ));

    *pdwSelectedDrives = 0;

    for ( nIndex = 0; nIndex <= nMaxIndex; nIndex++ )
        {
        if ( GetCheck ( nIndex ) )
            {
            // Retrieve the drive letter and convert it to the right bit
            // in the DWORD bitmask.

            chDrive = static_cast<TCHAR>( GetItemData ( nIndex ));

            // Internal check - letters are always kept as uppercase.
            ASSERT ( _istupper ( chDrive ));

            *pdwSelectedDrives |= ( 1 << ( chDrive - 'A' ));
            }
        }
}

void CDrivePickerListCtrl::GetSelectedDrives ( LPTSTR szSelectedDrives ) const
{
int   nIndex;
int   nMaxIndex = GetItemCount() - 1;
TCHAR szDrive[2] = { 0, 0 };

    // Need at least a 27-char buffer to hold A-Z and the terminating zero.
    ASSERT ( AfxIsValidAddress ( szSelectedDrives, 27 * sizeof(TCHAR) ));

    *szSelectedDrives = 0;

    for ( nIndex = 0; nIndex <= nMaxIndex; nIndex++ )
        {
        if ( GetCheck ( nIndex ) )
            {
            *szDrive = static_cast<TCHAR>( GetItemData ( nIndex ));

            lstrcat ( szSelectedDrives, szDrive );
            }
        }
}


/////////////////////////////////////////////////////////////////////////////
// CDrivePickerListCtrl other functions


//////////////////////////////////////////////////////////////////////////
//
// Function:    CommonInit
//
// Description:
//  _Really_ does all the initialization of the list control, sets up the
//  styles, and fills in the list with drives that match the flags specified
//  in the call to InitList().
//
// Input:
//  bLargeIcons: [in] Pass TRUE to put large icons in the control, or FALSE
//               to use small icons.
//  nFlags: [in] Same flags as InitList() takes.
//
// Returns:
//  Nothing.
//
//////////////////////////////////////////////////////////////////////////

void CDrivePickerListCtrl::CommonInit ( BOOL bLargeIcons, DWORD dwFlags )
{
    // These are the styles that we'll add to and remove from the 
    // list control.  Feel free to take out "LVS_SHOWSELALWAYS" below; I
    // have that in dwRemoveStyles because that's my personal preference.
    // The other styles listed should stay as-is, though.

DWORD dwRemoveStyles = LVS_TYPEMASK | LVS_SORTASCENDING | LVS_SORTDESCENDING |
                       LVS_OWNERDRAWFIXED | LVS_SHOWSELALWAYS;
DWORD dwNewStyles = LVS_REPORT | LVS_NOCOLUMNHEADER | LVS_SHAREIMAGELISTS |
                    LVS_SINGLESEL;

DWORD dwRemoveExStyles = 0;
DWORD dwNewExStyles = LVS_EX_CHECKBOXES;

    // The docs say that switching LVS_OWNERDATA on/off programatically is
    // unsupported.  Go back to the dialog editor and uncheck the "owner
    // data" checkbox on the list control's property sheet.
    ASSERT ( 0 == ( GetStyle() & LVS_OWNERDATA ));


    // Tweak the control styles so you don't have to remember to set them
    // in your dialog template.

    ModifyStyle ( dwRemoveStyles, dwNewStyles );

    ListView_SetExtendedListViewStyleEx ( GetSafeHwnd(),
                                          dwRemoveExStyles | dwNewExStyles,
                                          dwNewExStyles );


    // Empty the control, and if there are no columns, insert a column.
    
    DeleteAllItems();

LVCOLUMN rCol = { LVCF_WIDTH };

    if ( !GetColumn ( 0, &rCol ) )
        {
        InsertColumn ( 0, _T("") );
        }


    // Get the handle of the system image list - which list we retrieve
    // (large or small icons) is controlled by the bLargeIcons parameter.

SHFILEINFO sfi;
DWORD      dwRet;
HIMAGELIST hImgList;
UINT       uFlags = SHGFI_USEFILEATTRIBUTES | SHGFI_SYSICONINDEX;

    uFlags |= ( bLargeIcons ? SHGFI_LARGEICON : SHGFI_SMALLICON );
    
    dwRet = SHGetFileInfo ( _T("buffy.com"), FILE_ATTRIBUTE_NORMAL,
                            &sfi, sizeof(SHFILEINFO), uFlags );

    hImgList = reinterpret_cast<HIMAGELIST>( dwRet );

    ASSERT ( NULL != hImgList );

    // If this isn't the first call, detach our CImageList object from
    // the old image list and attach to the one whose handle we just
    // got from SHGetFileInfo().

    if ( NULL != m_ImgList.GetSafeHandle() )
        {
        m_ImgList.Detach();
        }

    VERIFY ( m_ImgList.Attach ( hImgList ));
    

    // Set the control to use the new image list.

    SetImageList ( &m_ImgList, LVSIL_SMALL );


    // Now, get the display name and icon for all drives on the system.

TCHAR szDriveRoot[] = _T("x:\\");
TCHAR cDrive;
DWORD dwDrivesOnSystem = GetLogicalDrives();
int   nIndex = 0;
UINT  uDriveType;

    // Sanity check - if this assert fires, how the heck did you boot? :)
    ASSERT ( 0 != dwDrivesOnSystem );


    uFlags = SHGFI_SYSICONINDEX | SHGFI_DISPLAYNAME | SHGFI_ICON;
    uFlags |= ( bLargeIcons ? SHGFI_LARGEICON : SHGFI_SMALLICON );

    for ( cDrive = 'A'; cDrive <= 'Z'; cDrive++, dwDrivesOnSystem >>= 1 )
        {
        if ( !( dwDrivesOnSystem & 1 ) )
            continue;

        // Get the type for the next drive, and check dwFlags to determine
        // if we should show it in the list.

        szDriveRoot[0] = cDrive;

		//MessageBox(&szDriveRoot[0]);

		if(!ToolClass::CheckUSBDriver(szDriveRoot))
			continue;

        //uDriveType = GetDriveType ( szDriveRoot );

        //switch ( uDriveType )
        //    {
        //    case DRIVE_NO_ROOT_DIR:
        //    case DRIVE_UNKNOWN:
        //        // Skip disconnected network drives and drives that Windows
        //        // can't figure out.
        //        continue;
        //    break;

        //    case DRIVE_REMOVABLE:
        //        if ( !( dwFlags & DDS_DLIL_REMOVABLES ))
        //            continue;
        //    break;

        //    case DRIVE_FIXED:
        //        if ( !( dwFlags & DDS_DLIL_HARDDRIVES ))
        //            continue;
        //    break;

        //    case DRIVE_REMOTE:
        //        if ( !( dwFlags & DDS_DLIL_NETDRIVES ))
        //            continue;
        //    break;

        //    case DRIVE_CDROM:
        //        if ( !( dwFlags & DDS_DLIL_CDROMS ))
        //            continue;
        //    break;

        //    case DRIVE_RAMDISK:
        //        if ( !( dwFlags & DDS_DLIL_RAMDRIVES ))
        //            continue;
        //    break;

        //    DEFAULT_UNREACHABLE;
        //    }


        // Now get the display name for the drive, and the position of the
        // drive's icon in the system image list.  The drive letter is stored
        // in each item's LPARAM for easy retrieval later.  Once all that
        // info has been retrieved, add an item to the list.

        if ( SHGetFileInfo ( szDriveRoot, 0, &sfi, sizeof(SHFILEINFO),
                             uFlags ))
            {
            InsertItem ( nIndex, sfi.szDisplayName, sfi.iIcon );
            SetItemData ( nIndex, cDrive );
            nIndex++;
            }
        }   // end for

    SetColumnWidth ( 0, LVSCW_AUTOSIZE_USEHEADER );
}
