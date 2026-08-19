# Sewa Rural Asset Management System

Cross-platform (.NET MAUI 9) asset management app for Sewa Rural — offline-first SQLite storage, role- and user-based menu rights, hierarchical location tracking, and QR-based asset verification.

## Solution structure

```
SewaRuralAM.sln
src/
  SewaRuralAM.Core             Domain entities, enums, repository/service interfaces (no MAUI/EF dependency)
  SewaRuralAM.Infrastructure   EF Core (SQLite) DbContext, repositories, seed data, services (auth, QR, PDF, menu rights)
  SewaRuralAM.App              .NET MAUI app — MVVM (CommunityToolkit.Mvvm), Shell navigation, XAML views
tests/
  SewaRuralAM.Tests            xUnit tests against an in-memory SQLite database
```

Architecture: Clean Architecture (Core → Infrastructure → App), Repository + Unit of Work pattern, dependency injection throughout, MVVM with CommunityToolkit.Mvvm source generators.

## Tech stack

| Concern | Library |
|---|---|
| UI framework | .NET MAUI 9 (Android, iOS, Windows) |
| MVVM | CommunityToolkit.Mvvm |
| Database | SQLite via EF Core (`Microsoft.EntityFrameworkCore.Sqlite`) |
| Password hashing | BCrypt.Net-Next |
| QR generation | QRCoder |
| QR scanning | ZXing.Net.MAUI (pinned to 0.4.0 — see note below) |
| PDF export | PdfSharpCore (see note below — QuestPDF was tried first and doesn't run on mobile) |
| Toasts | CommunityToolkit.Maui |
| Typeface | Poppins (Google Fonts, OFL) |
| Tests | xUnit + `Microsoft.Data.Sqlite` in-memory |

### A note on package versions

As of this build, the newest releases of `ZXing.Net.MAUI` / `ZXing.Net.MAUI.Controls` and `CommunityToolkit.Maui` on NuGet only target `net10.0`; they no longer ship `net9.0-android`/`-ios`/`-windows` assets. The project is pinned to the last versions published with net9 platform targets (ZXing 0.4.0, CommunityToolkit.Maui 12.3.0), so it restores and builds against the installed .NET 9 MAUI workloads. If/when the solution moves to .NET 10, re-evaluate and upgrade to the latest releases.

### A note on the PDF library

QuestPDF was the original choice but **does not support Android or iOS at all** — its native rendering package only ships binaries for `win-*`, `linux-*`, and `osx-*` runtime identifiers, so `QuestPDF.Settings.License = ...` throws `PlatformNotSupportedException` the instant it's touched on a phone. The project now uses **PdfSharpCore** instead (`SewaRuralAM.Infrastructure/Services/PdfService.cs`), which is pure managed code with no native dependency, plus a custom `IFontResolver` (`EmbeddedFontResolver.cs`) that reads Poppins straight out of an embedded resource so PDF text renders correctly without relying on system fonts. Both `GenerateAssetRegisterReport` and `GenerateAssetQrSheet` are covered by tests that actually generate a PDF and check the file signature.

## Running the app

```powershell
# Windows
dotnet build src\SewaRuralAM.App\SewaRuralAM.App.csproj -f net9.0-windows10.0.19041.0 -t:Run

# Android (emulator/device attached)
dotnet build src\SewaRuralAM.App\SewaRuralAM.App.csproj -f net9.0-android -t:Run

# Tests
dotnet test tests\SewaRuralAM.Tests\SewaRuralAM.Tests.csproj
```

iOS requires a paired Mac build host (standard MAUI requirement); build-only compilation was verified with `dotnet build -f net9.0-ios`.

### Default login

Seeded on first run (`DbInitializer.SeedAsync`):

- **User name:** `admin` / **Password:** `Admin@123` / **Role:** Administrator (full menu rights)
- **User name:** `manager` / **Password:** `Manager@123` / **Role:** Manager (add/edit menu rights, no delete/print on most menus)
- **User name:** `viewer` / **Password:** `Viewer@123` / **Role:** Viewer (read-only menu rights)

Sample data for manual QA is also seeded: 3 full 6-level location branches (Head Office → Building A → 2 floors → rooms → racks → shelves, plus a Warehouse branch), 8 assets spread across all 3 categories and every status (including Under Repair and Disposed), and verification history for several assets/locations so the Dashboard charts and verification reports have real data to show immediately.

The SQLite database file is created at `FileSystem.AppDataDirectory/sewarural.db` on first launch — no manual setup needed. Delete it (plus the `-wal`/`-shm` files alongside it) to force reseeding on next launch.

## Feature status

### Done
- Login, Change Password, Forgot Password (token-based reset). Signing in starts a **persistent session that survives an app restart and stays active until explicit Logout** (flyout footer) — this is unconditional, not gated by the Remember Me checkbox.
- Role-based **and** user-level Menu Rights (user-level rights override role-level for the same menu), enforced live against the Shell flyout — menus a user isn't authorized for are hidden, not just disabled. Menu Rights page supports editing both modes, with a searchable Role/User picker.
- Dashboard: 4 clickable stat cards (Total Assets, Total Locations, Verified, Pending) in a responsive 2x2 grid, + Assets-by-Category / Assets-by-Location bar charts, drawn on a `GraphicsView` (`Controls/BarChartView.cs`) rather than laid out with data-bound XAML, so rendering can't silently fail the way a bound `CollectionView`/`BoxView` combination can (see "Bugs found" below for why that mattered). Assets-by-Location shows each bar's **full breadcrumb chain** (e.g. "Head Office > Building A > Floor 1 > Room 101 > Rack 1 > Shelf A"), not just the leaf location name — the label is drawn above its bar and word-wraps up to 3 lines so long chains stay readable on any screen width.
- Asset CRUD (card-based list with status-colored pills and a verified badge), search/filter, dynamic category properties, QR generation, verification workflow with history log
- Location tree (6-level max, enforced), search, expand/collapse, **add-child directly from any node** in the tree (shows the resulting level before saving), add-root
- Asset-to-location assignment restricted to **exactly Level 6** locations (the deepest allowed), shown with a full breadcrumb chain (e.g. "Head Office > Building A > Floor 1 > Room 101 > Rack 1 > Shelf A"). A branch that hasn't been built out to Level 6 yet simply won't appear as an option — the Asset page shows an explicit warning when there are none.
- QR camera scanning (Android/iOS camera + location permissions declared in the manifest/Info.plist) recognizes **both** asset and location QR codes and drives the matching verification flow — assets verify against their assigned location; locations verify themselves directly (`LocationVerificationLog`, mirroring `VerificationLog`)
- QR **printing**: dedicated "Print Asset QR Codes" and "Print Location QR Codes" pages (search, multi-select via checkboxes, Select All/Clear) reachable from Reports and from the Assets/Locations toolbars — both export a multi-QR PDF sheet via `IPdfService.GenerateAssetQrSheet`. Location QR printing is restricted to Level 6 locations only.
- PDF export: Asset Register, Asset Verification Report, and Location Verification Report (date + verifying user for every scan), plus QR sheets — all work on Android/iOS/Windows (see PdfSharpCore note above) and **auto-open in the OS's default PDF viewer** after generating (`Services/PdfFileHelper.cs`)
- Last verification date and verifying user are shown on both the Asset Detail page and the Location Edit page, with a full history list on each
- Users & Roles is a **list page + separate add/edit page** (like Assets), not one combined form
- Reusable searchable dropdown control (`Controls/SearchablePicker`) — rebuilt as a fully self-contained inline expand-in-place control (see "Bugs found" below) — used for Category, Location, Parent Location, Role/User, and QR-print location-filter pickers everywhere
- **Toast notifications** on every save, delete, verify, login, and report action (success and failure)
- Branding pulled from the actual Sewa Rural diya logo: maroon/gold palette, app icon, splash screen, login page, and flyout header all use it; typeface is Poppins
- **Icons throughout**: flyout menu items, toolbar Add/Print buttons, the Location tree's Add/Edit row actions, and primary action buttons (Save/Delete/Verify/Logout/Sign In) all use Google's Material Icons font (`Controls/IconGlyphs.cs`), not text-only
- 25 passing unit tests: repository CRUD, location hierarchy, location breadcrumb-chain building, auth/session lifecycle, user-level vs. role-level menu rights precedence, PDF generation (including both verification reports), and the location-verification data flow

### Not yet built
- Location Summary / Missing Asset reports, Excel export
- Asset transfer history view (location mapping history exists in the data model, no dedicated UI yet)
- Release signing/store packaging config
- GPS capture during verification (permissions are declared; capture code not yet wired)

## Known limitations

- If an asset is currently mapped to a location that isn't exactly Level 6 (e.g. from before this constraint existed, or from a branch that was never extended to Level 6), the Asset Detail page's location field will show "not assigned" until you reassign it to a proper Level 6 location — this is a deliberate consequence of the Level-6-only rule, not a bug.
- The searchable dropdown's inner list uses reflection-based (not compiled) bindings — fine for the small lists it's used with, just not the fastest possible option.
- Auto-opening a generated PDF depends on the OS having a registered PDF viewer available to `Launcher.OpenAsync`; if none is available the file is still saved to app storage and the failure is swallowed (logged to debug output) rather than shown to the user, so it fails silently on a device with no PDF app installed.

## Bugs found and fixed across two passes (for context on why some code looks the way it does)

- **`SearchablePicker` selection did nothing / threw errors**: the picker's modal list handled taps via `TapGestureRecognizer` and pattern-matched `sender is Grid` — but for a `TapGestureRecognizer.Tapped` event, `sender` is the gesture recognizer itself, not the element it's attached to, so the match always failed. Every dropdown in the app was affected (Category, Location, Role, User, Parent). Fixed by switching to `CollectionView.SelectionChanged`, which doesn't have this ambiguity, and hardened both the picker and its modal against double-open/double-pop races; any residual exception now surfaces as a toast instead of failing silently.
- **Dashboard charts didn't render, twice**: first pass diagnosed this as `CollectionView` nested inside an unbounded `VerticalStackLayout`/`ScrollView` (a well-known MAUI trap) and switched to `BindableLayout` — that's a real, legitimate MAUI pitfall, but it evidently wasn't the whole story here since the charts were still reported broken afterwards. Rather than keep guessing at XAML-binding subtleties, the charts are now drawn directly on a `GraphicsView` canvas (`Controls/BarChartView.cs`), which has no data-binding/layout-nesting failure mode to hit in the first place.
- **Menu Rights save threw on new rows**: the code called `Update()` right after `AddAsync()` on the same entity, flipping its EF state from `Added` to `Modified` before it had ever been saved — EF then tried to `UPDATE` a row that didn't exist yet. Fixed by only calling `Update()` for rows that already existed.
- **QuestPDF crashed on Android/iOS**: see the PDF library note above — swapped to PdfSharpCore.
- **App crashed on Windows startup after refactoring `AppShell`/`App` to use constructor injection**: `App(AppShell shell)` resolved via `UseMauiApp<App>()`'s DI container triggered a native WinUI crash (`0xC000027B` inside `Microsoft.UI.Xaml.dll`) — isolated by bisection (it reproduced even with the flyout header/footer XAML stripped down to nothing). Reverted to the original pattern: `App` has a parameterless constructor and creates `new AppShell()` directly; `AppShell` resolves its own dependencies from `IPlatformApplication.Current!.Services` in its constructor instead of taking them as parameters.
- **Session didn't stay active**: originally only persisted across restarts when Remember Me was checked. Changed to always persist on login — a session now stays active until explicit Logout, full stop.
- **Raw Unicode/Private-Use-Area characters get mangled when written to files in this environment**: discovered while building `IconGlyphs.cs` — typing literal glyph characters directly resulted in double-UTF-8-encoded mojibake on disk. Worked around by writing `\uXXXX` C# escape sequences (plain ASCII) instead, generated via a small PowerShell script rather than typed directly, to guarantee the exact bytes that end up in the file.
- **`SearchablePicker` was rewritten entirely (third time's the charm)**: after the `CollectionView.SelectionChanged` fix still produced "getting error" reports for asset/location creation, rather than keep patching a component built around `Shell.Navigation.PushModalAsync` — a separate page, a cross-page `TaskCompletionSource`, and Shell's back-button handling all interacting — the control was rebuilt to never leave the current page at all. Tapping the field now just expands a search box and a *height-bounded* `CollectionView` directly beneath it, inside the same `ContentView`. No navigation stack, no modal lifecycle, no cross-page state — there's structurally much less left to go wrong.
- **`BarChartView` (the dashboard chart's second implementation) never updated after its first render**: its `ItemsSource` change handler checked `if (items is INotifyCollectionChanged)` — but `items` was the result of `.ToList()`, and `List<T>` never implements that interface, regardless of what the source collection was. The subscription was dead code; the chart only ever showed whatever was in the `ObservableCollection` at the exact moment of initial binding (typically empty, since that happens before the ViewModel's `LoadAsync` populates it). Fixed by checking the actual bound `newValue`/`oldValue` instead of a snapshot copy of it.
- **Login card and Dashboard stat cards looked cramped on narrow phones**: the Login page's `Frame` used `HorizontalOptions="Center"` with no fixed width, and `Entry` only declares a `MinimumWidthRequest` — with no explicit width to fill to, the `Frame` shrink-wrapped to a narrow box instead of using the available screen width. Fixed by switching it to `HorizontalOptions="Fill"` (still capped by `MaximumWidthRequest="380"` on wider screens, where MAUI centers a filled-but-capped view automatically). The Dashboard's 4 stat cards were in a single `*,*,*,*` row, which squeezed labels like "Pending Verification" onto phone-width screens; changed to a 2x2 grid instead.
