import React from 'react';
import { Stack, IStackTokens, IStackStyles } from '@fluentui/react';
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom'
import './App.css';
import { ConfigFiles } from './config/ConfigFiles';
import { PortSets } from './config/PortSets';
import { getSampleDataSource as getSampleConfigs } from './model/configs.sample';
import { getSampleDataSource as getSamplePortSets } from './model/portsets.sample';
import { NavMenu } from './NavMenu';

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

export const App: React.FunctionComponent = () => {
  const sampleConfigFiles = getSampleConfigs();
  const samplePortSets = getSamplePortSets();

  return (
    <Router>
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
          <Switch>
            <Route path="/config/files">
              <ConfigFiles dataSource={sampleConfigFiles} />
            </Route>
            <Route path="/config/portsets">
              <PortSets dataSource={samplePortSets} />
            </Route>
          </Switch>
        </Stack.Item>
      </Stack>
    </Router>
  );
};
