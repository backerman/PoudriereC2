import { StorybookConfig } from '@storybook/nextjs'

const config: StorybookConfig = {
    "stories": ["../src/**/*.stories.mdx", "../src/**/*.stories.@(js|jsx|ts|tsx)"],
    "addons": ["@storybook/addon-links", "@storybook/addon-essentials", "@storybook/addon-interactions"],
    "framework": {
        name: "@storybook/nextjs",
        options: {
            strictMode: true
        }
      },
      docs: {
        autodocs: true
      }    
}

export default config;