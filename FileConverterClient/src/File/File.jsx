import delSvg from '../images/del.svg';
import fileSvg from '../images/file.svg';
import pngSvg from '../images/png.svg';
import pdfSvg from '../images/pdf.svg';
import jpgSvg from '../images/jpg.svg';
import pptxSvg from '../images/pptx.svg';
import docSvg from '../images/doc.svg';
import docxSvg from '../images/docx.svg';
import zipSvg from '../images/zip.svg'

const getFileInfo = (fileName) =>{
    const parts = fileName.split('.');
    const extension = parts.length > 1 ? parts.pop().toLowerCase() : '';
    const name = parts.join('.');

    let icon;

    switch (extension){
      case 'png':
        icon = pngSvg;
        break;
      case 'jpg':
        icon = jpgSvg;
        break;
      case 'jpeg':
        icon = jpgSvg;
        break;
      case 'pdf':
        icon = pdfSvg;
        break;
      case 'pptx':
        icon = pptxSvg;
        break;
      case 'doc':
        icon = docSvg;
        break;
      case 'docx':
        icon = docxSvg;
        break;
      case 'zip':
        icon = zipSvg;
        break;
      default:
        icon = fileSvg; 
    }

    return {
      name: fileName,
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
          {
           props.isDel ?
              <button onClick={handleDelete}>
                <img className='w-4.5 relative left-16 top-3.5' draggable={false} src={delSvg}></img>
              </button>
              :
              null
          }            
            <img className='w-20' src={icon} draggable={false}></img>
            <div className="w-20 text-s text-fc-dark-gray text-center mt-2">{name.length > 5 ? name.substring(0,5)+'..'+ name.substring(name.lastIndexOf('.')): name }</div>
        </div>
    )
}

export default File