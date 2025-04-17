import { useState } from 'react'
import './App.css'
import axios from 'axios'
import conversionConfig from '../conversionConfig';
import Select from "react-select"
import MySelect from './MySelect/MySelect';
import jpgfile from './images/jpgfile.png'

function App() {

  const [files, setFiles] = useState([]);
  const [selectedOption, setSelectedOption] = useState(null);

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
        <form className='form rounded-xl border-1 border-dashed border-fc-dark-gray h-110 p-20'>
          <h1 className='text-center text-fc-dark-gray'>Бросать сюда</h1>
          <input type='file' className='input text-fc-dark-gray' multiple={true}
          onChange={handlerChange}
          />
        {files.length>0 && <ul className='file-list text-fc-dark-gray'>
          {files.map(({name}, id) =>(
            <div>
              <img className='h-15 w-15' src={jpgfile}></img>
              <li className='text-xs' key={id}>{name}</li>
            </div>
          ))}
          </ul>}
        </form>
      </div>
      <div className='flex-2/5 text-fc-gray my-auto ml-20 text-lg'>
        <div className={files.length==0 ? '': 'hidden'}>
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
        {files.length>0 ? 
        <div className='space-y-2 p-4'>
          <MySelect type = {files[0].name.split('.').pop()} selectedOption = {selectedOption} setSelectedOption={setSelectedOption}/>
        </div>
        : ""
        }        
      <button onClick={sendFiles} className='cursor-pointer bg-fc-orange rounded-lg hover:bg-fc-orange/80 text-white w-55 h-13 mx-5 my-10 font-bold text-lg'>Конвертировать</button>
      </div>
    </div>
    </>      
  )
}

export default App
