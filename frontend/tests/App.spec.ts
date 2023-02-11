import { test, expect } from '@playwright/test';

test('renders nav bar', async ({page}) => {
  await page.goto('http://localhost:3003');
  const linkElement = page.getByText(/Configuration/i);
  await expect(linkElement).toBeVisible();
});
