#include "pch.h"

#include "Context.h"
#include "MediaStreamObserver.h"

namespace unity
{
namespace webrtc
{

    MediaStreamObserver::MediaStreamObserver(webrtc::MediaStreamInterface* stream)
        : ::webrtc::MediaStreamObserver(
              stream,
              [this](webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream) {
                  this->OnAudioTrackAdded(track, stream);
              },
              [this](webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream) {
                  this->OnAudioTrackRemoved(track, stream);
              },
              [this](webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream) {
                  this->OnVideoTrackAdded(track, stream);
              },
              [this](webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream) {
                  this->OnVideoTrackRemoved(track, stream);
              })
    {
    }

    void
    MediaStreamObserver::OnVideoTrackAdded(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        for (auto callback : m_listOnAddTrack)
        {
            callback(stream, track);
        }
    }

    void
    MediaStreamObserver::OnAudioTrackAdded(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        for (auto callback : m_listOnAddTrack)
        {
            callback(stream, track);
        }
    }

    void
    MediaStreamObserver::OnVideoTrackRemoved(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        for (auto callback : m_listOnRemoveTrack)
        {
            callback(stream, track);
        }
    }

    void
    MediaStreamObserver::OnAudioTrackRemoved(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        for (auto callback : m_listOnRemoveTrack)
        {
            callback(stream, track);
        }
    }

    void MediaStreamObserver::RegisterOnAddTrack(DelegateMediaStreamOnAddTrack callback)
    {
        m_listOnAddTrack.push_back(callback);
    }

    void MediaStreamObserver::RegisterOnRemoveTrack(DelegateMediaStreamOnRemoveTrack callback)
    {
        m_listOnRemoveTrack.push_back(callback);
    }

} // end namespace webrtc
} // end namespace unity
