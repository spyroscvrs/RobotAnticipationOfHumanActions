   M S B A M L   `   `   `  ����  BodyBasics-WPF4    .Microsoft.Samples.Kinect.BodyBasics.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Body Basics��5   	   $	��600��5   	   $	��700��6    � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush��%�   &	        &	 #     &	 F     �� 5	      $ 	#ff6e6e6e��6.   �� 5
      $ 	#ff52318f��60   �� 5      $ 	#ff00BCF2��6.   5      .��5      � $��	10 0 10 0q�6	    � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5      �� $��0��6   $��Images\Status.png��6   $
��None=�64   $��Center=�6C   $��0 0 0 5q�6`   (� 5      $��1��6   $��Center=�6   .��5      �� $��UniformToFill=�6-   ��6   �� *ImageSource+6-   5      �� 5      -   5         ��Name$ Record��5      $��Record��5       �� 	ClickMode$ Press=�5       $��10q�5!      $	��150��5"      $
��Left=�5#      
�������5$      �� 5&   
   -   5        $ Hands��5&      $��Hands��5'      $ Press=�5)      $��10q�5*      $	��150��5+      $��Center=�5,      
�������5-      �� 5/   
   -   5        $
 Head��5/      $
��Head��50      $ Press=�52      $��10q�53      $	��150��54      $��Right=�55      
�������56      �� 59   
   -   5        $��	statusBar��59   ?   $��2��6   $��Stretch=�6!   $��Bottom=�6P   
�������#��[  6~   .@�5:      �� ��6   �� *
StatusText+�� 5<      $��0��6   $��0 0 -1 0q�6%   $��Center=�67   $��Bottom=�6T   #��[  6o   $��Segoe UI)�6�   $��18(�6�   Skeleton tracking6�   5@      5A      5C      