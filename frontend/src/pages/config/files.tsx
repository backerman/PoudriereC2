import { getSampleDataSource } from 'src/models/configs.sample';
import { ConfigFiles } from './ConfigFiles';

const samples = getSampleDataSource();

export default function files() {
    return <ConfigFiles dataSource={samples} showDeleted={true}></ConfigFiles>
}