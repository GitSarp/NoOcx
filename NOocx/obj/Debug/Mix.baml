   M S B A M L   `   `   `  ����@  <NOocx, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:NOocxNOocx  5   	   V RAForge.Controls, Version=2.2.5.0, Culture=neutral, PublicKeyToken=a8ac264d1dc6b9d9J6clr-namespace:AForge.Controls;assembly=AForge.ControlsAForge.Controls 5   	       	NOocx.Mix   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:NOocx     5   	   Iaforge6clr-namespace:AForge.Controls;assembly=AForge.Controls     5   	   -   5          � Title$	  Mix��5	   	   $	��600��6   $	��900��6"   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   ]� -   5         ]�Name$ menubar��5   *   $	��Top=�6   $��Stretch=�6;   
�������.+�5      �� -   5         ��Name$ 	videoMenu��5   .   $	��150��6    �� Text$ 	视频源��6#   6q   �� 5      -   5        $ 	mediaType��5   .   $	��100��6   $ 	分辨率��6#   6A   �� 5      -   5         ��Name$ currfileName��5      $	��150��6*   6;   �� 5      -   5        $��zoomasc��5   7   $��+��6   $y�	Collapsed=�6F   6_   �� 5      -   5        $��zoomdesc��5   !   $��-��6   $y�	Collapsed=�6H   6a   5      � 5    
   $��1��6    � ColumnDefinitions 5!      �� 5"      $��* �6#   6/   �� 5#      $
��Auto �6#   62   .+�5&      7  1System.Windows.Forms.Integration.WindowsFormsHost  -   5          Name$ wfhost��5&         Child. 5'      '  !AForge.Controls.VideoSourcePlayer  -	   5        videoPlayer��5'   B   5(        5*      -
   5        $ 
canvashost��5*   3   $y�Hidden=�6   . 5+         AForge.Controls.PictureBox  -   5        
canvas��5+   6   5,      �� 5-      	 �� Orientation$	 Vertical=�6   $��1��60   
�������.+�5.      �� -   5        $��采集��5.      $��40��6'   $��30��62   6V   �� 5/      $��采集��6   $��30��6'   65   �� 50      $��采集��6   $��30��6&   64   �� 51      $��采集��6   $��30��6'   65   52      53      �� 55   
   -   5        $��2��55      $��Disabled=�6$   $��Visible=�6K   .��56   
   �� -   5        $��bottom_image��56   F   $	 
Horizontal=�6   
�������5A      5B      5T      5U      