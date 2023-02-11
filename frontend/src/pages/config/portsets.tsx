import { getSampleDataSource } from 'src/models/portsets.sample';
import { PortSets } from 'src/components/config/PortSets';

const samples = getSampleDataSource();

export default function files() {
    return <PortSets dataSource={samples}></PortSets>
}