import React from 'react';
import { Stack, IStackTokens, IStackStyles } from '@fluentui/react';
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom'
import './App.css';
import { ConfigFiles } from './ConfigFiles';
import { getSampleDataSource } from './model/configs.sample';
import { NavMenu } from './NavMenu';

const stackTokens: IStackTokens = { childrenGap: 15 };
const stackStyles: Partial<IStackStyles> = {
  root: {
    margin: '0 auto',
    textAlign: 'left',
    color: '#605e5c',
  },
};

export const App: React.FunctionComponent = () => {
  const sampleData = getSampleDataSource();

  return (
    <Router>
      <Stack
        horizontal
        horizontalAlign="start"
        tokens={stackTokens}
        styles={stackStyles}
        verticalFill={true}>
        <NavMenu />
        <Switch>
          <Route exact path="/">
            <ConfigFiles dataSource={sampleData} />
          </Route>
        </Switch>
      </Stack>
    </Router>
  );
};
