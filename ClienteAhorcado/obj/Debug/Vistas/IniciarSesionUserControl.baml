   M S B A M L   `   `   `  ����  ClienteAhorcado5    /ClienteAhorcado.Vistas.IniciarSesionUserControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   $	��461��5   	   $	��380��6   .��5	      � .+�5   
   �� $��/Images/pinzarron.jpeg��6   $��UniformToFill=�5        J� Opacity$
  0.93Q�5       K� ZIndex$ 0��5      �� 5   
    �� CornerRadius$ 18��6   $��16q�6#   $ 1��6/   ��5      �� 5   
    �� Color$ 	#CCFFFFFF��6   ._�5      � $��24q�6    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $��* �6$   .+�5      �� $��0��6   $��	0,20,0,28q�6*   $��Center=�6=   .+�5      �� -   5         ��Name$ tblockTitulo��5          InicioSesion_Titulo#���   5       $��30(�5       $
��Bold"�5       ���#23272F �� TextAlignment$ Center=�5!       �� 5"      -   5        $ tblockSubtitulo��5"         InicioSesion_Descripcion#���  5#       $��15(�5$       ���#8b97a2$��0,6,0,0q�5&       $ Center=�5'       5(      �� 5+      $��1��6   $	��Top=�6*   $��Center=�6B   $	��280��6_   .+�5,      �� -   5        $ tblockCorreo��5,         Glb_CorreoUsuario#���  66   $��SemiBold"�6a   $��0,0,0,6q�6w   �� 5-      -   5         ��Name$ tbCorreo��5-      $��37��60   $
��10,5q�6<   ���#cad1e1$��1q�5.   4   ���#f6f8fa$��0,0,0,2q�5/   3   �� 50      -   5        $ tblockErrorCorreo��50       $�� ��6;   
���  ��$��12(�6T   �� 52      -   5        $ tblockContrasena��52         Glb_Contrasenia#���  6:   $��SemiBold"�6c   $��0,10,0,6q�6y   � 53      .+�54      G� -   5         G�Name$ 
pbPassword��54   &   $��37��6:   $��10q�6F   ���#cad1e1$��1q�55   8   ���#f6f8fa$��0,0,40,0q�56   7   $y�Visible=�57   "   67   �� 58      -   5        $ tbPasswordVisible��58   "   $��37��6=   $��0,0,40,0q�6I   $��
10,5,10,10q�6[   ���#cad1e1$��1q�59   8   ���#f6f8fa$��Center=�5;   "   $y�	Collapsed=�6=   6T   �� 5<      -	   5        	 ��Name$	 btnVerPassword��5<   !   $��30��69   $��30��6D   $��Right=�6P   $��Center=�6l   $
��👁��5>   !   
������ ��5@   !   Z� 5A      �� 5B      -
   5        $ tblockErrorPassword��5B       $�� ��6=   $��0,2,0,8q�6E   
���  ��$��12(�6g   $��Bottom=�6u   �� 5C      -   5        $	 btnIniciarSesion��5C        InicioSesion_Boton_Iniciar#���  67      EstiloBotonPrincipalSinIcono#��[ 5E      $��42��5F      $��0,10,0,8q�6)   �� 5G      -   5        $	 btnRegistrarse��5G         InicioSesion_Boton_Registrar#���  66    ! EstiloBotonSecundarioSinIcono#��[ 5I      $��40��5J      
������ ���#3498db���#3498db6X   5K      � 5M      -   5        
 �Name$
 IdiomaToggleContainer��5M      $��Bottom=�5N      $��Right=�62   $��0,0,-38,-39q�5O      $��40��6,   $��40��67   $��1��6C   .+�5P      �� -   5         ��Name$ 
imgEnglish��5P      $��/Images/english.png��5Q      $��30��5R      $��30��6'    � Cursor$
 Hand��5S      $��Right=�5T      $��Bottom=�68   $y�Visible=�5U      $  1Q�5V      $��0q�5X      �� 5Y      -   5        $ 
imgEspanol��5Y      $��/Images/espanol.png��5Z      $��30��5[      $��30��6'   $
 Hand��5\      $��Right=�5]      $��Bottom=�68   $y�	Collapsed=�5^      $  1Q�5_      $��0q�5a      5b      5c      5d      5e      5f      