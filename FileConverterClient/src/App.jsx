import { useState } from 'react'
import './App.css'
import axios from 'axios'
import MySelect from './MySelect/MySelect';
import jpgfile from './images/jpgfile.png'
import downloadfile from './images/downloadIcon.png'
import { useRef } from 'react';

function App() {

  const [files, setFiles] = useState([]);
  const [selectedOption, setSelectedOption] = useState(null);


  const fileInputRef = useRef(null);

  const handleClick = () => {
    fileInputRef.current.click();
  };

  const handlerChange = (e) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]){
      setFiles([...e.target.files])
    }
  }


  const sendFiles = async (e) =>{

    const requestBody =  {}

    if (selectedOption.isArray){
      requestBody.files = files
    }
    else{
      requestBody.file = files[0]
    }

    selectedOption.additionalParams.forEach(param => {
      requestBody[param.property] = 2
    });

    const response = await axios.post(`/api/${selectedOption.api}`,
      requestBody,
      {
        headers:{
          'Content-Type': 'multipart/form-data'
        },
        responseType: "blob",
      }) 

      const disposition = response.headers["content-disposition"];
      let fileName = "downloaded-file";
      if (disposition && disposition.indexOf("attachment") !== -1) {
        const matches = disposition.match("filename=(.+);");
        console.log(matches)
        if (matches != null && matches[1]) {
          fileName = matches[1].replaceAll("\"","");
        }
      }
      

      const url = window.URL.createObjectURL(new Blob([response.data]));

      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", fileName);
      document.body.appendChild(link);
      link.click();
  }

  return (
    <>
    <header className='bg-fc-orange text-white h-20 text-4xl font-bold py-4 pl-16'>
      File Converter
    </header>
    <div className='flex flex-row my-20'>
      <div className=' flex-3/5 bg-fc-light-gray rounded-xl p-5 h-120 mx-20'>
        <div className='rounded-xl border-1 border-dashed border-fc-dark-gray h-110 p-20'>
          {files.length==0 ? 
            <form className='form'>
              <img className='h-15 w-15 mx-auto mt-25 mb-3 cursor-pointer' onClick={handleClick} src={downloadfile}></img>
              <div className='text-center text-s text-fc-dark-gray'>Перетащите файл(ы) в эту область или
                <br/> нажмите&nbsp;
                <label className='cursor-pointer text-fc-orange underline'>сюда
                  <input 
                    type='file' 
                    className='hidden' 
                    multiple={true} 
                    onChange={handlerChange}
                    ref ={fileInputRef}
                  /> 
              </label>
              , чтобы выбрать файлы
              </div>                 
            </form>
          :
          ""
          } 
        </div>
      </div>
      <div className='flex-2/5 text-fc-gray my-auto ml-20 text-lg'>
        {files.length>0 ? 
        <div className='space-y-2 my-27'>
          <MySelect type = {files[0].name.split('.').pop()} selectedOption = {selectedOption} setSelectedOption={setSelectedOption}/>
        </div>
        : 
        <div>
          <ul className='list-disc'>
                File Converter предоставляет<br/>
                возможность  конвертации:
            <li className='ml-7'>pdf в word и jpg</li>
            <li className='ml-7'>word в pdf</li>
            <li className='ml-7'>jpg в png и pdf</li> 
            <li className='ml-7'>png в jpg</li> 
            <li className='ml-7'>PowerPoint в pdf</li> 
          </ul>
          <ul className='list-disc'>А так же:
            <li className='ml-7'>Объединение pdf</li>
            <li className='ml-7'>Разделение pdf</li>
          </ul> 
        </div> 
        }  
        <button onClick={sendFiles} className={files.length>0 ? 'active-button': 'passive-button'}>Конвертировать</button>
      </div>
    </div>
    </>      
  )
}

export default App
