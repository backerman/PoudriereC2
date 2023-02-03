import { test, expect } from '@playwright/test';

import React from 'react';
import App from 'src/pages/_app';
import { initializeIcons } from '@fluentui/react';

initializeIcons();

test('renders nav bar', async ({page}) => {
  await page.goto('http://localhost:3003');
  const linkElement = page.getByText(/Configuration/i);
  await expect(linkElement).toBeVisible();
});
