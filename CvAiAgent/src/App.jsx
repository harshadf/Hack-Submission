import React from 'react';
import { Box } from '@mui/material';
import ChatBox from './ChatBox';
import InputFileUpload from './InputFileUpload';
import JobDescription from './JobDescription';
import AiTypingAnimation from './AiTypingAnimation';
import ResponsiveAppBar from './ResponsiveAppBar';

function App() {
  return (
    <Box sx={{ height: '100vh', width: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyItems: 'center' }}>
      {/* Navbar at the top */}
      <ResponsiveAppBar />

      {/* Main Content */}
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'row',
          flexGrow: 1,
          width: '100%',
          p: 2,
          gap: 2,
          overflow: 'hidden',
        }}
      >
        {/* Left side: File Upload and Job Description */}
        <Box
          sx={{
            width: '300px',
            height: 'fit-content',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            p: 2,
            pl: 30,
            gap: 3,
          }}
        >
          <InputFileUpload />
          <JobDescription />
        </Box>

        {/* Right side: Chat Area and AI Animation */}
        <Box
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'row',
            gap: 2,
            pl: 15,
          }}
        >
          {/* AI Animation */}
          <Box
            sx={{
              flex: 1,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              minWidth: '200px',
              width: '100%',
              px: 5
            }}
          >
            <AiTypingAnimation />
          </Box>
          
          {/* Chat Box */}
          <Box sx={{ flex: 3 }}>
            <ChatBox />
          </Box>
        </Box>
      </Box>
    </Box>
  );
}

export default App;
