import { test, expect } from '@playwright/test';

test('renders nav bar', async ({page}) => {
  await page.goto('http://localhost:3003');
  const linkElement = page.getByRole('link', { name: 'Configuration', exact: true });
  await expect(linkElement).toBeVisible();
});
