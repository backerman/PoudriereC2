import { getTheme, IStackItemStyles, IStackStyles, IStackTokens, ITextStyles, Stack, Text } from "@fluentui/react";
import { ReactElement, ReactNode } from "react"
import { NavMenu } from "./NavMenu"
import { TopBar } from "./TopBar";

const stackTokens: IStackTokens = {
  childrenGap: 15,
  padding: '2em',
};

const stackStyles: Partial<IStackStyles> = {
  root: {
    margin: '0 auto',
    textAlign: 'left',
    color: '#605e5c',
    width: '100%',
  },
};

const contentStyles: Partial<IStackStyles> = {
  root: {
    minWidth: '0' // otherwise it won't shrink when window does
  }
}

interface IHasChildren {
  children?: ReactNode
}

export default function Layout({ children }: IHasChildren): ReactElement {
  return (
    <>
      <Stack>
        <Stack.Item>
          <TopBar/>
        </Stack.Item>
        <Stack
          horizontal
          horizontalAlign="start"
          tokens={stackTokens}
          styles={stackStyles}
          verticalFill={true}>
          <Stack.Item>
            <NavMenu />
          </Stack.Item>
          <Stack.Item grow styles={contentStyles}>
            {children}
          </Stack.Item>
        </Stack>
      </Stack>
    </>
  )
}
