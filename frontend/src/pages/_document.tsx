import { InjectionMode, Stylesheet, resetIds } from '@fluentui/react';
import Document, { Html, Head, Main, NextScript, DocumentContext, DocumentInitialProps } from 'next/document'

const stylesheet = Stylesheet.getInstance();

type MyDocInitialProps = DocumentInitialProps & {
  serializedStylesheet: string;
  styleTags: string;
}

export class MyDocument extends Document<MyDocInitialProps> {
  static async getInitialProps({ renderPage }: DocumentContext)
    : Promise<MyDocInitialProps> {
    resetIds();

    // eslint-disable-next-line react/display-name
    const page = await renderPage(App => props => {
      return <App {...props} />;
    });

    return { ...page,
      styleTags: stylesheet.getRules(true),
      serializedStylesheet: stylesheet.serialize() };
  }

  render() {
    return (
      <Html lang="en">
        <Head>
          <style type="text/css" dangerouslySetInnerHTML={{ __html: this.props.styleTags }} />
          <script type="text/javascript" dangerouslySetInnerHTML={{ __html: `
            window.FabricConfig = window.FabricConfig || {};
            window.FabricConfig.serializedStylesheet = ${this.props.serializedStylesheet};
          ` }} />
        </Head>
        <body>
          <Main />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <link rel="icon" href="/favicon.ico" />
          <NextScript />
        </body>
      </Html>
    )  
  }
}

export default MyDocument;
