#include "pch.h"

#include "Context.h"
#include "MediaStreamObserver.h"

namespace unity
{
namespace webrtc
{

    MediaStreamObserver::MediaStreamObserver(webrtc::MediaStreamInterface* stream, Context* context)
        : ::webrtc::MediaStreamObserver(stream)
        , m_context(context)
    {
        this->SignalVideoTrackAdded.connect(this, &MediaStreamObserver::OnVideoTrackAdded);
        this->SignalAudioTrackAdded.connect(this, &MediaStreamObserver::OnAudioTrackAdded);
        this->SignalVideoTrackRemoved.connect(this, &MediaStreamObserver::OnVideoTrackRemoved);
        this->SignalAudioTrackRemoved.connect(this, &MediaStreamObserver::OnAudioTrackRemoved);
    }

    void
    MediaStreamObserver::OnVideoTrackAdded(webrtc::VideoTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        m_context->AddRefPtr(track);
        for (auto callback : m_listOnAddTrack)
        {
            callback(stream, track);
        }
    }

    void
    MediaStreamObserver::OnAudioTrackAdded(webrtc::AudioTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        m_context->AddRefPtr(track);
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
