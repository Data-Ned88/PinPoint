   M S B A M L   `   `   `  ����E  APinpointUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:PinpointUI.tabsPinpointUI.tabs  5      *    $PinpointUI.tabs.OneNoteManagementTab   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      /localclr-namespace:PinpointUI.tabs     5      .��5	      � -   5        $��OneNoteTabMainGrid��5	        � RowDefinitions  5
   
   �� 5      $��8* �6   �� 5      $	��18* �6   �� 5      $	��70* �6   �� 5      $��8* �6   .+�5   
   �� 
�������6   �� 5   
   $��3��6   
�������6!   �� 5   
   -   5         ��Name$ OneNoteTabBackToWelcome��5      $��<< Back to Welcome Page��62   $��20,0,0,0q�6Z   $	��200��6l   $��23��6x   $
��Left=�6�   6�   � 5   
   -   5        $��OneNoteTabSecondRow��5      $��1��6*    � ColumnDefinitions 5      �� 5      $	��40* �6#   �� 5      $	��60* �6#   .+�5      �� 
�������6   �� 5      $C��=Select a Notebook from your available OneNote notebooks below��6   $��Center=�6]   $��	10,0,10,0q�6x   $
��Wrap=�6�   �� 5      $��1��6   
�������6(   �� 5      -   5         ��Name$ txthdrAvailableSections��5      $��1��69   $��Center=�6I   $��	10,0,10,0q�6d   $
��Wrap=�6w   
�������6�   5      � 5   
   -   5        $��OneNoteTabThirdRow��5      $��2��6)    5      �� 5      $	��40* �6#   �� 5       $	��60* �6#   .+�5"      �� 
�������
�������6)   �� 5#      -   5         ��Name$ listAvailableNotebooks��5#      $��10,20,50,20q�66    �� SelectionMode$ Single=�6K    �� SelectedIndex$ 0��6b    �� DisplayMemberPath$ @name��6�   ��6�   �� *Notebookslist+6�   � 5$      -   5        $ ��OnNoteTabThirdRowColTwo_LE��5$      $��1��65   $y�Visible=�6E     5%      �� 5&      $	��75* �6$   �� 5'      $	��25* �6$   .+�5)      �� 
�������
�������6-   � 5*      -   5         �Name$ gridNewSectionDEntry��5*      $y�Hidden=�65     5+      �� 5,      $	��50* �6(   �� 5-      $	��50* �6(   .+�5/      �� -	   5        	 ��Name$	 newSectionLabel��5/      $��Bottom=�65   $��
10,5,10,20q�6P   �� 50      -
   5        
 ��Name$
 newSectionName��50      $��1��66   $	��Top=�6C   $��
10,5,20,20q�6[   51      &   System.Windows.Controls.DataGrid  52      -   5          Name$ gridSections��53      $��10q�54         CanUserAddRows .     CanUserDeleteRows .     SelectionMode$ Single=�56         AutoGenerateColumns .     
IsReadOnly . ��55      �� *SectionsList+   Columns 5:      0  *System.Windows.Controls.DataGridTextColumn  5;      ,  &System.Windows.Controls.DataGridColumn   Header$
 Name��6-      Width5  /System.Windows.Controls.DataGridLengthConverter$	 40* 6[   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6;   �� *SectionName+6[     5<      $ Section Group��6-   $	 20* 6e    6D   �� *SectionGroup+6e   4  .System.Windows.Controls.DataGridCheckBoxColumn  5=      $ Locked��61   $	 15* 6^    6A   �� *
IsLocked+6^   4  .System.Windows.Controls.DataGridTemplateColumn  5?      $ IsValid��61   $	 25* 6B      CellTemplate 5@      �� 5A   "   .R�5B   &   �� $ isValidTextBlock��60    � Cursor$
 Help��6z   ��6J   �� * IsValidPinPointInstanceDisplay+ � ToolTip 5C   *   _� 5D   .    _�Name$ isValidTextBlockTT��66   ��6R   �� *IsValidTooltip+5F   '    �� Triggers 5G   &   �� 5H   *    �� Value$	 Yes��6i    �� Binding 66   �� * IsValidPinPointInstanceDisplay+.a�5I   .   ��  �� 
TargetName$ isValidTextBlock��65    �� Property � �� �� Value
 � � �6i   �� 5J   .   $ isValidTextBlock��65    � ��$	 ☑��6c   5L   +   �� 5M   *   $ No��6i    66   �� * IsValidPinPointInstanceDisplay+.a�5N   .   �� $ isValidTextBlock��65    � ��
 �  ��6i   �� 5O   .   $ isValidTextBlock��65    � ��$	 ☒��6c   5P   +   �� 5Q   *   $ NA��6i    66   �� * IsValidPinPointInstanceDisplay+.a�5R   .   �� $ isValidTextBlock��65    � ��
 �� ��6i   �� 5S   .   $ isValidTextBlock��65    � ��$ ?��6c   �� 5T   .   $ isValidTextBlock��65    � ��$
 Bold"�6i   5U   +   5W   #   5Y      5[      � 5\      -   5        $  ONT_ActionButtonsLoadExist��5\      $��1��6;    5]      �� 5^      $	��50* �6+   �� 5_      $	��50* �6+   .+�5a      �� 
�������
�������61   �� 5b      
�������
�������$��1��6;   �� 5c      -   5        $ btnRefreshSectionInfo��5c      $��Refresh Sections��6<   $��10,15,10,15q�6W   6l   �� 5d      -   5        $ btnLoadSection��5d      $��Load Section��6W   $��10,15,10,15q�6n   $��1��6�   ��65   �� *fnLoadSection+6�   5e      � 5f      -   5        $  ONT_ActionButtonsCreateNew��5f      $��1��6;   $y�Hidden=�6H    5g      �� 5h      $	��50* �6+   �� 5i      $	��50* �6+   .+�5k      �� 
�������6    �� 5l      
�������$��1��61   �� 5m      -   5        $ btnCreateSection��5m      $��Create New Section��67   $��10,15,10,15q�6T   $��1��6i   6y   5n      5o      5p      5q      5r      