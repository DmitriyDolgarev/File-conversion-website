import { useState } from 'react'
import './App.css'

function App() {

  const [files, setFiles] = useState([]);

  const handlerChange = (e) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]){
      setFiles([...e.target.files])
    }
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
            <li key={id}>{name}</li>
          ))}
          </ul>}
        </form>
      </div>
      <div className='flex-2/5 text-fc-gray my-auto ml-20 text-lg'>
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
        <button className='bg-fc-gray rounded-lg text-white w-55 h-13 mx-5 my-10 font-bold text-lg'>Конвертировать</button>
      </div>
    </div>
    </>      
  )
}

export default App
