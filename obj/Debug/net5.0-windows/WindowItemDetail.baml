   M S B A M L   `   `   `  ????F  BSNSelfOrder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:SNSelfOrderSNSelfOrder  5   	   "    SNSelfOrder.WindowItemDetail   5   	   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   _ [Microsoft.Xaml.Behaviors, Version=1.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a3b+http://schemas.microsoft.com/xaml/behaviors  5   	   %localclr-namespace:SNSelfOrder  5   	     ? Left$  0??5	      
 ? Top$ 0??6#    ? WindowStyle$
 None=?5
   	    ? WindowStartupLocation$ Manual=?5   	    ? ShowInTaskbar$ False??5   	    ? AllowsTransparency$
 True??5   	    ? Title$ WindowItemDetail??5   	   $
??1080??6"   $
??1080??60   '  !System.Windows.Shell.WindowChrome   WindowChrome 5        5   
      GlassFrameThickness$ 0q?6   	   CornerRadius$	 0??6/   
   CaptionHeight$
 0Q?6@   ??5      ?? 5   
    ?? Color$ 	#60efefef??6   .??5      ?    LayoutItemRootGridStyle#??[  6    ? RowDefinitions 5   
   ?? 5      $??2* ?6   ?? 5      $??7* ?6   ?? 5        BottomHeight#??[ 6   .+?5   
   ?? -   5         ??Name$ mBackroundImage??5      $??3??5      $??Stretch=?6!   $??Stretch=?5      $
??Fill=?5      $??0q?5!      ??5       ?? *BackgroundImagePath+5"      ? 5#   
   $??0??6     GirdItemDescStyle#??[ 6    ? ShowGridLines$ False??6H    ? ColumnDefinitions 5$      ?? 5%      $??3* ?6#   ?? 5&      $??7* ?6#    5(      ?? 5)      $??* ?6    ?? 5*      $??2* ?6    .+?5-      ?? $??0??6   $??0??6(   $??2??65     ItemImagStyle#??[ 6F   ??5.      ?? 5/       j? Stretch$ UniformToFill=?6G    ?? ImageSource 6!   ?? *ItemImagePath+6G   51      ? 52      $??0??6   $??1??6     53      ?? 54      $??8* ?6'   ?? 55      $??2* ?6'   .+?57      ?? -   5         ??Name$ ItemCaption??57      $??0??62     ItemNameStyle#??[ 6`   ??6D   ?? *
ItemName+6`   ?? 58      -   5        $ 
ItemAmount??58      $??1??62     ItemPriceStyle#??[ 6s   ??6C   ?? *
ItemAmount+ ?? StringFormat$ C2??6s   59      ?? 5:      -   5        $ ItemDescription??5:      $??1??61   $??1??6>     ItemDescriptionStyle#??[ 6o   ??6N   ?? *ItemDescription+6o   5;      ? 5<   
   -   5         ?Name$ GridItemModifer??5<      $??1??6     GirdItemModifierStyle#??[ 66   $ False??6e    5=      ?? 5?      $
??Auto ?6    ?? 5@      $??* ?6    .+?5B      ? -   5        $ ItemVarietyGrid??5B       $??0??6     GirdItemVarietyStyle#??[ 6:   .+?5C      ?? -   5         ??Name$ ItemVarietyListBox??5C       	 ItemVarietyListBoxStyle#??[	 5E      $??Hidden=?5F      $??Disabled=?5G      $??False??5H       ?? PanningMode$ HorizontalOnly=?5I       
 ItemVarietyListItemStyle#??[
 5J        ItemVarietyDataTemplate#??[ 5K      ??5D      ?? *ItemVarietys+ ?? Mode$ TwoWay=? ?? UpdateSourceTrigger$ PropertyChanged=?5Q      5R      ? 5T      -   5        $ ItemModSetGrid??5T       $??1??6   $??0q?6;   .+?5U      ?? -	   5        $ ModSetListBox??5U      $??Disabled=?5W      $??Hidden=?5X      $??False??5Y      $ VerticalOnly=?5Z        ItemModSetListItemStyle#??[ 5[      ??5V      ?? *SelectedVariety.ModifierSets+$ TwoWay=?$ PropertyChanged=???5\      ?? 5]      .R?5_      ??  ??Name$ myStackPanel??6G   $
??1000??6*   $
??1000??6:   $??Stretch=?5`   )   $
??Left=?5a   )   .+?5c   &   ? $??0??6,   ???	#FFE06216.+?5d   *   ??   ModSetCaptionStyle#??[ 5e   8   ??5d   8   ??  ?? Path$ Caption?5e   8   5f   '   ? 5g   &   $??1??6+   .+?5h   &   ?? $ ModifierListBox??5i   )   $??Disabled=?5k   )   $??Hidden=?5l   )   $??False??5m   )   $
 None=?5n   )     ModifierTemplate#??[ 5o   )   ??5j   )   ?? $ 	Modifiers???5p   *   ?? 5q   .   .R?5r   2   ? $
??Left=?5s   A   5t   /   5v   '   5w   #   5y      5|      5~      5?      5?      5?      5?      