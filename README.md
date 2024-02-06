# PinPoint
UCOVI Pinpoint PIN and Password manager for OneNote

This is a *work in progress* .NET Framework app written in C# with a WPF GUI that is designed to help a user save their most important PINs and passwords as a locked OneNote section, and do additionals such as:
- Read the PINs and Passwords of an existing OneNote Section created by this app, so that they can add, edit and delete from their password section and publish updates back to OneNote.
- Run a report on the individual strength of passwords and PINs based on suitable algorithms of password strength (eg. Hyve's days to crack passwords based on length and character complexity)
- Run a report on the collective strength of the PINs and passwords of the sections as a whole, based on how frequently the same password or stem of the same password appears.
- Import a list of passwords as CSV with mappable columns to the interface for onward publish to OneNote.
- Auto-generate a string alphanumeric password

## Work done so far
- Built generic classes and functions to interact with the OneNote desktop app using LINQ, and to save text updates to a OneNote page in various fonts and font sizes.
- Built the frame and 2 tabs of a GUI in XAML to point the user to the OneNote notebook they would like to use and section within said notebook they would like to edit.

## Work to do
- GUI 3rd tab to edit, delete, and add new password entries to a OneNote Section
- Advanced C# classes to publish pages to OneNote with sophisticated table styling
- Functionality for password strength reporting
- List import via CSV
- Password auto-generator
- Publish software as application .exe
