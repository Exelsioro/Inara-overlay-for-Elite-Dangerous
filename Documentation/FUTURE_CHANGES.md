# Future Changes

## Critical Priority
1. Decompose `InaraTools/InaraParserUtils.LegParsing.cs`.
2. Decompose `ED_Inara_Overlay/Windows/TradeRouteWindow.xaml.cs`.
3. Decompose `ED_Inara_Overlay/UserControls/TradeRouteCard.xaml.cs`.
4. Decompose `InaraTools/InaraCommunicator.cs`.

## High Priority
1. Decompose `ED_Inara_Overlay/Services/ThemeManager.cs`.
2. Decompose `ED_Inara_Overlay/Utils/UIHelpers.cs`.
3. Decompose `ED_Inara_Overlay/Utils/WindowsAPI.cs`.

## Medium Priority
1. Continue decomposition of `ED_Inara_Overlay/Windows/MainWindow.xaml.cs`.
2. Continue decomposition of `ED_Inara_Overlay/Windows/MainWindow.OverlayOrchestration.cs`.

## Low Priority
1. Split `ED_Inara_Overlay/Resources/UIStyles.xaml` into thematic resource dictionaries.

## Notes
1. Keep current behavior unchanged during decomposition.
2. After each change block, run `dotnet build` for `InaraTools` and `ED_Inara_Overlay`.
