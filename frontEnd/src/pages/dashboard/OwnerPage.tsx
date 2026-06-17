import { FaUserCog } from 'react-icons/fa';
import PageAccessTemplate from '../../components/dashboard/page-access/PageAccessManagement';

const OwnerPage = () => {
  return (
    <div className='pageTemplate2'>
      <PageAccessTemplate color='#3b3549' icon={FaUserCog} role='Owner' />
    </div>
  );
};

export default OwnerPage;