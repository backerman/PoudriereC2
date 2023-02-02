import { mergeStyles } from '@fluentui/react';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import { createRoot } from 'react-dom/client';
import reportWebVitals from './reportWebVitals';

import { App } from './App';

initializeIcons();

// Inject some global styles
mergeStyles({
  ':global(body,html,#root)': {
    margin: 0,
    padding: 0,
    height: '100vh',
  },
});

const container = document.getElementById('root');
const root = createRoot(container!);
root.render(<App />);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
