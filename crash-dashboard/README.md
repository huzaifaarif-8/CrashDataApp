# crash-dashboard (Angular frontend)

Angular 21 frontend for the CrashDataApp analytics dashboard. It is built into `../CrashDataApp/wwwroot` and served by the .NET backend at `http://localhost:5050` — no separate dev server is needed in normal use.

## Tech stack

- Angular 21 standalone components
- Chart.js (bar, doughnut, horizontal bar charts)
- zone.js polyfill (required for change detection with native `fetch`)
- Native `fetch` + `Promise.all` for API calls

## Source layout

```
src/
├── app/
│   ├── app.ts              # root component, data fetching, chart rendering
│   ├── app.html            # template (stat cards, charts, tables)
│   ├── app.css             # component styles (dark theme)
│   ├── app.config.ts       # Angular providers (HttpClient)
│   └── services/
│       └── crash-api.service.ts   # TypeScript interfaces for API response shapes
├── styles.css              # global CSS custom properties (dark palette)
└── main.ts                 # bootstrapApplication entry point
```

## Building

```bash
npm install
npx @angular/cli@21 build --configuration development
```

Output goes to `../CrashDataApp/wwwroot`. Restart the .NET server (`dotnet run --urls http://localhost:5050` from `../CrashDataApp`) to pick up the new build.

## How it connects to the backend

All API calls use relative paths (e.g. `/api/crashes/summary`). Because Angular is served from the same origin as the API, there are no CORS requests — the browser treats everything as same-origin.

The `angular.json` `outputPath` is set to `{ "base": "../CrashDataApp/wwwroot", "browser": "" }` which flattens the build directly into wwwroot (Angular 17+ otherwise nests output in a `browser/` subdirectory).
