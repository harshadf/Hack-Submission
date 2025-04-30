import React, { useEffect, useState } from 'react';
import { Box, Typography, Paper, List, ListItem, TextField, Button } from '@mui/material';
import { usePromptStore } from './store/usePromptStore';
import axios from '../src/axios';
import Skeleton from 'react-loading-skeleton'
import 'react-loading-skeleton/dist/skeleton.css'

function ChatBox() {
  const [chatMessages, setChatMessages] = useState([]);
  const secondText = usePromptStore((state) => state.secondText);
  const firstText = usePromptStore((state) => state.firstText);
  const setSecondText = usePromptStore((state) => state.setSecondText);
  const getCombinedText = usePromptStore((state) => state.getCombinedText);
  const [aiResponse, setAiResponse] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isJobDescriptionSent, setIsJobDescriptionSent] = useState(false);

  useEffect(() => {
    const aiResponseText = { from: 'ai', text: `${aiResponse}` };
    if (aiResponseText.text == '') return;
    console.log(aiResponseText);
    setChatMessages((prev) => [...prev, aiResponseText]);
  }, [aiResponse]);

  useEffect(() => {
    setIsJobDescriptionSent(false);
  }, [firstText]);

  const handleSendChat = async () => {
    if (firstText != '' && !isJobDescriptionSent) {
      setIsJobDescriptionSent(true);
    }
    const prompt = isJobDescriptionSent ? secondText : getCombinedText();
    console.log("prompt text: ", prompt);
    setIsLoading(true);

    const userMessage = { from: 'user', text: prompt };
    setChatMessages((prev) => [...prev, userMessage]);
    setSecondText('');

    await axios.post(`/ChatWithAI/`, { prompt }, {
      headers: {
        'Content-Type': 'application/json'
      }
    })
    .then((res) => {
      console.log('res: ', res);
      setAiResponse(res.data);
      console.log('aiResponse state: ', aiResponse);
      setIsLoading(false);
    })
    .catch((er) => {
      console.log(er);
      setIsLoading(false);
    })
  };

  return (
    <Box sx={{ 
      height: '100%',
      display: 'flex', 
      flexDirection: 'column', 
      alignItems: 'center', 
      justifyContent: 'flex-start', 
      p: 2
    }}>
      
      <Box sx={{ 
        width: '95%',
        maxWidth: '1400px',
        height: '100%', 
        display: 'flex', 
        flexDirection: 'column' 
      }}>

        <Typography variant="h6" gutterBottom>
          Chat with AI Agent
        </Typography>
        <Paper 
          variant="outlined" 
          sx={{ 
            flexGrow: 1, 
            overflowY: 'auto', 
            p: 2, 
            mb: 2, 
            display: 'flex', 
            flexDirection: 'column',
            minHeight: '450px',
            maxHeight: '400px'
          }}
        >
          <List>
            {chatMessages.map((msg, idx) => (
              <ListItem 
                key={idx} 
                sx={{
                  display: 'flex',
                  justifyContent: msg.from === 'user' ? 'flex-end' : 'flex-start',
                }}
              >
                <Box 
                  sx={{
                    bgcolor: msg.from === 'user' ? '#1976d2' : '#e0e0e0',
                    color: msg.from === 'user' ? '#fff' : '#000',
                    px: 2,
                    py: 1,
                    borderRadius: 2,
                    maxWidth: '70%',
                    wordBreak: 'break-word',
                  }}
                >
                  {msg.text.replace(/【\d+:\d+†source】/g, '').split('\n').map((line, i) => (
                  <React.Fragment key={i}>
                    {line}
                    <br />
                  </React.Fragment>
                ))}
                </Box>
              </ListItem>
              ))}
              <Box 
                  sx={{
                    px: 2,
                    py: 1,
                    borderRadius: 2,
                    maxWidth: '70%',
                    wordBreak: 'break-word',
                  }}
                >
                  {isLoading && (<Skeleton count={5} />) }
              </Box>
          </List>
        </Paper>

        <Box display="flex" gap={1}>
          <TextField
            fullWidth
            variant="outlined"
            size="small"
            value={secondText}
            onChange={(e) => setSecondText(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSendChat()}
          />
          <Button
            variant="contained"
            onClick={handleSendChat}
            disabled={secondText.trim() === ''}
          >
            Send
          </Button>
        </Box>

      </Box>
    </Box>
  );
}

export default ChatBox;
