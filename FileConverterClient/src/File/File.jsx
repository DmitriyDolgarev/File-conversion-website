import delSvg from '../images/del.svg';
import fileSvg from '../images/file.svg';
import pngSvg from '../images/png.svg';
import pdfSvg from '../images/pdf.svg';
import jpgSvg from '../images/jpg.svg';
import pptxSvg from '../images/pptx.svg';
import docSvg from '../images/doc.svg';

const getFileInfo = (fileName) =>{
    const parts = fileName.split('.');
    const extension = parts.length > 1 ? parts.pop().toLowerCase() : '';
    const name = parts.join('.');

    let icon;

    switch (extension){
      case 'png':
        icon = pngSvg;
        break;
      case 'jpg' || 'jpeg':
        icon = jpgSvg;
        break;
      case 'pdf':
        icon = pdfSvg;
        break;
      case 'pptx':
        icon = pptxSvg;
        break;
      case 'doc' || 'docx':
        icon = docSvg;
        break;
      default:
        icon = fileSvg; 
    }

    return {
      name: name,
      icon: icon
    };
} 

function File(props) {
    const {name, icon} = getFileInfo(props.filename)
    
    const handleDelete = (e) => {
      e.stopPropagation();
      props.onDelete(props.filename); 
    };

    return (
        <div>
            <button onClick={handleDelete}>
              <img className='w-4.5 relative left-16 top-3.5' src={delSvg}></img>
            </button>
            <img className='w-20' src={icon}></img>
            <div className="text-s text-fc-dark-gray text-center mt-2">{name.length > 8 ? name.substring(0,8)+'...' : name }</div>
        </div>
    )
}

export default File