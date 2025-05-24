function Instruction(props) {
    return (
        <div className='lg:mt-5 lg:mb-10 ml-10'>
            <ul className='list-disc'>
                File Converter предоставляет<br />
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
    )
}

export default Instruction