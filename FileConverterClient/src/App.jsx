import { useState } from 'react'
import './App.css'
import axios from 'axios'
import { useRef } from 'react';
import downloadfile from './images/downloadIcon.svg'
import MySelect from './MySelect/MySelect';
import File from './File/File'
import sync from './images/Synchronize.svg'



function App() {

  const [files, setFiles] = useState([]);
  const [selectedOption, setSelectedOption] = useState(null);
  const [splitParam, setSplitParam] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [downLink, setDownLink] = useState(null)

  const fileInputRef = useRef(null);

   const handleDelete = (filename) => {
    setFiles(files.filter(file => file.name !== filename));
  };



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
    setIsLoading(true);

    const requestBody = new FormData();

    if (selectedOption.isArray){
      files.forEach(file => {
        requestBody.append('files', file);
      });
    }
    else{
      requestBody.append('file', files[0])
    }

    selectedOption.additionalParams.forEach(param => {
      requestBody.append(param.property, splitParam)
    });

    const response = await axios.post(`/api/${selectedOption.api}`,
      requestBody,
      {
        headers:{
          'Content-Type': 'multipart/form-data'
        },
        responseType: "blob",
      }) 

      setSelectedOption(null)

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

      
      //document.body.appendChild(link);
      //link.click();
      
      setIsLoading(false)
  }


  return (
    <>
  {console.log(files)}
    <header className='bg-fc-orange text-white h-20 text-4xl font-bold py-4 pl-16'>
      File Converter
    </header>
    <div className='flex flex-row my-20'>
      <div className=' flex-3/5 bg-fc-light-gray rounded-xl p-5 h-120 mx-20 '>
        <div className='rounded-xl border-1 border-dashed border-fc-dark-gray h-110 p-3'>
          {files.length==0 ? 
            <form className='form'>
              <img className='h-15 w-15 mx-auto mt-40 mb-3 cursor-pointer' onClick={handleClick} src={downloadfile}></img>
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
            <div className="container h-106 overflow-y-auto">
              <div className="grid grid-cols-5 gap-2">
                {files.map((file, index) => (
                  <div key={index} className="flex flex-col items-center">
                    <File filename={file.name} onDelete={handleDelete}></File>
                  </div>
                ))}
              </div>
            </div>
          } 
        </div>
      </div>
      <div className='flex-2/5 text-fc-gray my-auto ml-20 text-lg'>
        {files.length>0 ? 
        <div className='space-y-2 mt-30'>
          { !isLoading  ?
            <div>
              <MySelect type = {files[0].name.split('.').pop()} selectedOption = {selectedOption} setSelectedOption={setSelectedOption}/>
              {console.log(selectedOption)}
              <div className={`my-15 flex items-center ${selectedOption?.conversionType === 'split' ? '' : 'invisible'}`}>
                <span className="text-lg ml-1 mr-3 text-fc-dark-gray">Введите страницы:</span>
                <input
                  placeholder="1-3; 5, 9-7"
                  className="bg-fc-light-gray focus:outline-none text-lg py-1 pl-2 ml-1 rounded-lg border border-gray-300 w-35"
                  onChange={(e) => setSplitParam(e.target.value.trim())}
                  type="text"
                  value={splitParam}
                />
              </div>
            </div>
            :
            <div className='mt-36 mb-14 '>
              <img src={sync} className='mx-30 h-20 spin-animation' ></img>
              <div className='mx-25 mt-6 text-fc-gray'>Конвертируем..</div>
            </div>
          }
          
        </div> 
        : 
        <div className='mt-16 ml-7'>
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
        <button onClick={sendFiles} className={files.length==0 || isLoading ? 'passive-button': 'active-button'}>Конвертировать</button>
      </div>
    </div>
    </>      
  )
}

export default App
