   M S B A M L   `   `   `  ����E  APinpointUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:PinpointUI.tabsPinpointUI.tabs  5      *    $PinpointUI.tabs.OneNoteManagementTab   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      /localclr-namespace:PinpointUI.tabs     5      .��5	      � -   5        $��OneNoteTabMainGrid��5	        � RowDefinitions  5
   
   �� 5      $��8* �6   �� 5      $	��18* �6   �� 5      $	��70* �6   �� 5      $��8* �6   .+�5   
   �� 
�������6   �� 5   
   $��3��6   
�������6!   �� 5   
   -   5         ��Name$ OneNoteTabBackToWelcome��5      $��<< Back to Welcome Page��62   $��20,0,0,0q�6Z   $	��200��6l   $��23��6x   $
��Left=�6�    � Cursor$
 Hand��6�    � ToolTip 5      _� 5      .��6   %#Click to go back to the last screen5      5      � 5   
   -   5        $��OneNoteTabSecondRow��5      $��1��6*    � ColumnDefinitions 5      �� 5      $	��40* �6#   �� 5      $	��60* �6#   .+�5      �� 
�������6   �� 5      $C��=Select a Notebook from your available OneNote notebooks below��6   $��Center=�6]   $��	10,0,10,0q�6x   $
��Wrap=�6�   $��14(�6�   �� 5       $��1��6   ���#6868686(   �� 5!      -   5         ��Name$ txthdrAvailableSections��5!      $��1��69   $��Center=�6I   $��	10,0,10,0q�6d   $
��Wrap=�6w   
�������$��Medium"�6�   $��14(�6�   5"      � 5#   
   -   5        $��OneNoteTabThirdRow��5#      $��2��6)    5$      �� 5%      $	��40* �6#   �� 5&      $	��60* �6#   .+�5(      �� 
�������
�������6)   �� 5)      -   5         ��Name$ listAvailableNotebooks��5)      $��10,20,50,20q�66    �� SelectionMode$ Single=�6K    �� SelectedIndex$ 0��6b   	 �� DisplayMemberPath$	 @name��6�   ��6�   �� *Notebookslist+6�   � 5*      -   5        $ ��OnNoteTabThirdRowColTwo_LE��5*      $��1��65   $y�Visible=�6E     5+      �� 5,      $	��75* �6$   �� 5-      $	��25* �6$   .+�5/      �� 
�������
�������6-   � 50      -   5        
 �Name$
 gridNewSectionDEntry��50      $y�Hidden=�65     51      �� 52      $	��50* �6(   �� 53      $	��50* �6(   .+�55      �� -	   5         ��Name$ newSectionLabel��55      $��Bottom=�65   $��
10,5,10,20q�6P   �� 56      -
   5         ��Name$ newSectionName��56      $��1��66   $	��Top=�6C   $��
10,5,20,20q�6[   57      &   System.Windows.Controls.DataGrid  58      -   5          Name$ gridSections��59      $��10q�5:         CanUserAddRows .     CanUserDeleteRows .     SelectionMode$ Single=�5<         AutoGenerateColumns .     
IsReadOnly . ��5;      �� *SectionsList+   Columns 5@      0  *System.Windows.Controls.DataGridTextColumn  5A      ,  &System.Windows.Controls.DataGridColumn   Header$
 Name��6-      Width5  /System.Windows.Controls.DataGridLengthConverter$	 40* 6[   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6;   �� *SectionName+6[     5B      $ Section Group��6-   $	 20* 6e    6D   �� *SectionGroup+6e   4  .System.Windows.Controls.DataGridCheckBoxColumn  5C      $ Locked��61   $	 15* 6^    6A   �� *
IsLocked+6^   4  .System.Windows.Controls.DataGridTemplateColumn  5E      $ IsValid��61   $	 25* 6B      CellTemplate 5F      �� 5G   "   .R�5H   &   �� $ isValidTextBlock��60   $
 Help��6z   ��6J   �� * IsValidPinPointInstanceDisplay+ 5I   *   _� 5J   .    _�Name$ isValidTextBlockTT��66   ��6R   �� *IsValidTooltip+5L   '    �� Triggers 5M   &   �� 5N   *    �� Value$	 Yes��6i    �� Binding 66   �� * IsValidPinPointInstanceDisplay+.a�5O   .   ��  �� 
TargetName$ isValidTextBlock��65    �� Property � �� �� Value
 � � �6i   �� 5P   .   $ isValidTextBlock��65    � ��$	 ☑��6c   5R   +   �� 5S   *   $ No��6i    66   �� * IsValidPinPointInstanceDisplay+.a�5T   .   �� $ isValidTextBlock��65    � ��
 �  ��6i   �� 5U   .   $ isValidTextBlock��65    � ��$	 ☒��6c   5V   +   �� 5W   *   $ NA��6i    66   �� * IsValidPinPointInstanceDisplay+.a�5X   .   �� $ isValidTextBlock��65    � ��
 �� ��6i   �� 5Y   .   $ isValidTextBlock��65    � ��$ ?��6c   �� 5Z   .   $ isValidTextBlock��65    � ��$
 Bold"�6i   5[   +   5]   #   5_      5a      � 5b      -   5        $ 
 ONT_ActionButtonsLoadExist��5b      $��1��6;    5c      �� 5d      $	��50* �6+   �� 5e      $	��50* �6+   .+�5g      �� 
�������
�������61   �� 5h      
�������
�������$��1��6@   �� 5i      -   5        $ btnRefreshSectionInfo��5i      $��Refresh Sections��6<   $��10,15,10,15q�6W   $
 Hand��6�   �� 5j      -   5        $ btnLoadSection��5j      $��Load Section��6W   $��Medium"�6n   $��10,15,10,15q�6�   $��1��6�   $��14(�6�   ��65   �� *fnLoadSection+6�   5k      � 5l      -   5        $ 
 ONT_ActionButtonsCreateNew��5l      $��1��6;   $y�Hidden=�6H    5m      �� 5n      $	��50* �6+   �� 5o      $	��50* �6+   .+�5q      �� 
�������6    �� 5r      
�������$��1��61   �� 5s      -   5        $ btnCreateSection��5s      $��Create New Section��67   $��10,15,10,15q�6T   $��1��6i   $
 Hand��6�   5t      5u      5v      5w      5x      