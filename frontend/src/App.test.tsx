import React from 'react';
import { act, render, screen } from '@testing-library/react';
import { App } from './App';
import { initializeIcons } from '@fluentui/react';

initializeIcons();

it('renders nav bar', async () => {
  expect.assertions(1);
  await act(async () => {render(<App />)});
  const linkElement = screen.getByText(/Configuration/i);
  expect(linkElement).toBeInTheDocument();
});
